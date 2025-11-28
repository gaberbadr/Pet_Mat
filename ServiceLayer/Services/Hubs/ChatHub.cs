using System.Collections.Concurrent;
using CoreLayer.Dtos.Messag;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Messages;
using CoreLayer.Enums;
using CoreLayer.Helper.Documents;
using CoreLayer.Service_Interface.Messag;
using CoreLayer.Service_Interface.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RepositoryLayer.Data.Context;

namespace petmat.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMessagingService _messagingService;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatHub> _logger;

        private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();

        public ChatHub(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMessagingService messagingService,
            INotificationService notificationService,
            IConfiguration configuration,
            ILogger<ChatHub> logger)
        {
            _context = context;
            _userManager = userManager;
            _messagingService = messagingService;
            _notificationService = notificationService;
            _configuration = configuration;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId == null)
            {
                await base.OnConnectedAsync();
                return;
            }

            try
            {
                // Mark all old connections as inactive first
                var oldConnections = await _context.UserConnections
                    .Where(c => c.UserId == userId && c.IsActive)
                    .ToListAsync();

                foreach (var oldConn in oldConnections)
                {
                    oldConn.IsActive = false;
                    oldConn.DisconnectedAt = DateTime.UtcNow;
                }

                //  Create new active connection
                var connection = new UserConnection
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = userId,
                    ConnectedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.UserConnections.AddAsync(connection);
                await _context.SaveChangesAsync();

                // Cleanup old connections immediately (keep only last 3)
                await CleanupOldConnectionsAsync(userId);

                _onlineUsers[userId] = Context.ConnectionId;

                await Clients.All.SendAsync("UserOnlineStatus", new
                {
                    UserId = userId,
                    IsOnline = true
                });

                var unreadMessages = await _messagingService.GetUnreadCountAsync(userId);
                var unreadNotifications = await _notificationService.GetUnreadCountAsync(userId);

                await Clients.Caller.SendAsync("UnreadCounts", new
                {
                    UnreadMessagesCount = unreadMessages,
                    UnreadNotificationsCount = unreadNotifications
                });

                var conversations = await _messagingService.GetConversationsAsync(userId);
                await Clients.Caller.SendAsync("LoadConversations", conversations);

                _logger.LogInformation($" User {userId} connected: {Context.ConnectionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OnConnectedAsync for user {userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;

            if (userId != null)
            {
                try
                {
                    var connection = await _context.UserConnections
                        .FirstOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);

                    if (connection != null)
                    {
                        connection.IsActive = false;
                        connection.DisconnectedAt = DateTime.UtcNow;
                        _context.UserConnections.Update(connection);
                        await _context.SaveChangesAsync();
                    }

                    _onlineUsers.TryRemove(userId, out _);

                    await Clients.All.SendAsync("UserOnlineStatus", new
                    {
                        UserId = userId,
                        IsOnline = false,
                        LastSeen = DateTime.UtcNow
                    });

                    _logger.LogInformation($"⌛ User {userId} disconnected: {Context.ConnectionId}");

                    //  Cleanup happens here
                    await CleanupOldConnectionsAsync(userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in OnDisconnectedAsync for user {userId}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        //  Keep only last 3 connections per user
        private async Task CleanupOldConnectionsAsync(string userId)
        {
            try
            {
                var connectionsToDelete = await _context.UserConnections
                    .Where(c => c.UserId == userId && !c.IsActive)
                    .OrderByDescending(c => c.DisconnectedAt ?? c.ConnectedAt)
                    .Skip(3) // Keep last 3 inactive
                    .ToListAsync();

                if (connectionsToDelete.Any())
                {
                    _context.UserConnections.RemoveRange(connectionsToDelete);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"🧹 Cleaned up {connectionsToDelete.Count} old connections for user {userId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cleaning up connections for user {userId}");
            }
        }

        public async Task LoadConversation(string otherUserId, int pageIndex = 1, int pageSize = 20)
        {
            var userId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(otherUserId))
            {
                await Clients.Caller.SendAsync("Error", "Invalid user IDs");
                return;
            }

            if (userId == otherUserId)
            {
                await Clients.Caller.SendAsync("Error", "Cannot load conversation with yourself");
                return;
            }

            try
            {
                var otherUser = await _userManager.FindByIdAsync(otherUserId);
                if (otherUser == null || !otherUser.IsActive)
                {
                    await Clients.Caller.SendAsync("Error", "User not found or inactive");
                    return;
                }

                var filterParams = new MessageFilterParams
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                var messages = await _messagingService.GetConversationMessagesAsync(
                    userId, otherUserId, filterParams);

                await Clients.Caller.SendAsync("ConversationLoaded", new
                {
                    OtherUserId = otherUserId,
                    Messages = messages.Data,
                    TotalCount = messages.Count,
                    PageIndex = messages.PageIndex,
                    PageSize = messages.PageSize,
                    HasMore = (messages.PageIndex * messages.PageSize) < messages.Count
                });

                _logger.LogInformation($"Conversation loaded: {userId} with {otherUserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading conversation");
                await Clients.Caller.SendAsync("Error", "Failed to load conversation");
            }
        }

        public async Task SendPrivateMessage(
                    string receiverId,
                    string message,
                    string messageType = "Text",
                    string contextType = null,
                    int? contextId = null)
        {
            var senderId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(senderId) || string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("Error", "Invalid message data");
                return;
            }

            try
            {
                if (!Enum.TryParse<MessageType>(messageType, true, out var msgType))
                {
                    msgType = MessageType.Text;
                }

                var messageContextType = MessageContextType.General;
                if (!string.IsNullOrEmpty(contextType))
                {
                    Enum.TryParse(contextType, true, out messageContextType);
                }

                if (msgType != MessageType.Text)
                {
                    await Clients.Caller.SendAsync("Error", "Media messages must be sent using the HTTP upload endpoint");
                    return;
                }

                var dto = new SendMessageDto
                {
                    ReceiverId = receiverId,
                    Content = message.Trim(),
                    Type = msgType,
                    ContextType = messageContextType,
                    ContextId = contextId
                };

                var result = await _messagingService.SendMessageAsync(senderId, dto);

                if (result == null || !result.Success)
                {
                    await Clients.Caller.SendAsync("Error", "Failed to send message");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        public async Task MarkMessageAsRead(int messageId)
        {
            var currentUserId = Context.UserIdentifier;
            if (currentUserId == null) return;

            try
            {
                await _messagingService.MarkAsReadAsync(messageId, currentUserId);

                var message = await _context.Messages.FindAsync(messageId);
                if (message != null)
                {
                    await Clients.User(message.SenderId).SendAsync("MessageRead", new
                    {
                        MessageId = messageId,
                        ReadBy = currentUserId,
                        ReadAt = DateTime.UtcNow
                    });

                    var unreadCount = await _messagingService.GetUnreadCountAsync(currentUserId);
                    await Clients.Caller.SendAsync("UnreadMessagesCount", unreadCount);

                    var conversations = await _messagingService.GetConversationsAsync(currentUserId);
                    await Clients.Caller.SendAsync("ConversationsUpdated", conversations);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task MarkConversationAsRead(string otherUserId)
        {
            var currentUserId = Context.UserIdentifier;
            if (currentUserId == null) return;

            try
            {
                await _messagingService.MarkConversationAsReadAsync(currentUserId, otherUserId);

                await Clients.User(otherUserId).SendAsync("ConversationRead", new
                {
                    ReadBy = currentUserId,
                    ReadAt = DateTime.UtcNow
                });

                var unreadCount = await _messagingService.GetUnreadCountAsync(currentUserId);
                await Clients.Caller.SendAsync("UnreadMessagesCount", unreadCount);

                var conversations = await _messagingService.GetConversationsAsync(currentUserId);
                await Clients.Caller.SendAsync("ConversationsUpdated", conversations);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task BlockUser(string userId)
        {
            var currentUserId = Context.UserIdentifier;
            if (currentUserId == null) return;

            try
            {
                await _messagingService.BlockUserAsync(currentUserId, userId);
                await Clients.Caller.SendAsync("UserBlocked", new
                {
                    Success = true,
                    Message = "User blocked successfully",
                    BlockedUserId = userId
                });

                var conversations = await _messagingService.GetConversationsAsync(currentUserId);
                await Clients.Caller.SendAsync("ConversationsUpdated", conversations);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task UnblockUser(string userId)
        {
            var currentUserId = Context.UserIdentifier;
            if (currentUserId == null) return;

            try
            {
                await _messagingService.UnblockUserAsync(currentUserId, userId);
                await Clients.Caller.SendAsync("UserUnblocked", new
                {
                    Success = true,
                    Message = "User unblocked successfully",
                    UnblockedUserId = userId
                });

                var conversations = await _messagingService.GetConversationsAsync(currentUserId);
                await Clients.Caller.SendAsync("ConversationsUpdated", conversations);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task CheckUserOnlineStatus(string userId)
        {
            bool isOnline = _onlineUsers.ContainsKey(userId);
            DateTime? lastSeen = null;

            if (!isOnline)
            {
                var lastConnection = await _context.UserConnections
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.DisconnectedAt ?? c.ConnectedAt)
                    .FirstOrDefaultAsync();

                lastSeen = lastConnection?.DisconnectedAt ?? lastConnection?.ConnectedAt;
            }

            await Clients.Caller.SendAsync("UserOnlineStatusResponse", new
            {
                UserId = userId,
                IsOnline = isOnline,
                LastSeen = lastSeen
            });
        }
    }
}