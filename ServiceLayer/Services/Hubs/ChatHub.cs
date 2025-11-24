using System;
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

        // ✅ OPTIMIZED: Store only userId -> single ConnectionId (last connection)
        // When user connects from multiple devices, we keep only the latest active connection
        private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();
        private static readonly object _lock = new object();

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
                // ✅ OPTIMIZED: Reuse existing connection or create new one
                var existingConnection = await _context.UserConnections
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

                if (existingConnection != null)
                {
                    // Update existing connection
                    existingConnection.ConnectionId = Context.ConnectionId;
                    existingConnection.ConnectedAt = DateTime.UtcNow;
                    existingConnection.DisconnectedAt = null;
                    _context.UserConnections.Update(existingConnection);
                }
                else
                {
                    // Create new connection
                    var connection = new UserConnection
                    {
                        ConnectionId = Context.ConnectionId,
                        UserId = userId,
                        ConnectedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _context.UserConnections.AddAsync(connection);
                }

                await _context.SaveChangesAsync();

                // ✅ OPTIMIZED: Store single connection per user
                _onlineUsers[userId] = Context.ConnectionId;

                // Broadcast online status
                await Clients.All.SendAsync("UserOnlineStatus", new
                {
                    UserId = userId,
                    IsOnline = true
                });

                // Send unread counts
                var unreadMessages = await _messagingService.GetUnreadCountAsync(userId);
                var unreadNotifications = await _notificationService.GetUnreadCountAsync(userId);

                await Clients.Caller.SendAsync("UnreadCounts", new
                {
                    UnreadMessagesCount = unreadMessages,
                    UnreadNotificationsCount = unreadNotifications
                });

                // Send conversations list
                var conversations = await _messagingService.GetConversationsAsync(userId);
                await Clients.Caller.SendAsync("LoadConversations", conversations);

                _logger.LogInformation($"✅ User {userId} connected: {Context.ConnectionId}");
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
                    // ✅ OPTIMIZED: Mark connection as inactive and set disconnection time
                    var connection = await _context.UserConnections
                        .FirstOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);

                    if (connection != null)
                    {
                        connection.IsActive = false;
                        connection.DisconnectedAt = DateTime.UtcNow;
                        _context.UserConnections.Update(connection);
                        await _context.SaveChangesAsync();
                    }

                    // Remove from online users dictionary
                    _onlineUsers.TryRemove(userId, out _);

                    // Broadcast offline status
                    await Clients.All.SendAsync("UserOnlineStatus", new
                    {
                        UserId = userId,
                        IsOnline = false,
                        LastSeen = DateTime.UtcNow
                    });

                    _logger.LogInformation($"❌ User {userId} disconnected: {Context.ConnectionId}");

                    // ✅ CLEANUP: Delete old inactive connections (keep only last 5 per user)
                    await CleanupOldConnectionsAsync(userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in OnDisconnectedAsync for user {userId}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ✅ CLEANUP: Keep only the last 5 connections per user
        private async Task CleanupOldConnectionsAsync(string userId)
        {
            try
            {
                var connectionsToDelete = await _context.UserConnections
                    .Where(c => c.UserId == userId && !c.IsActive)
                    .OrderByDescending(c => c.DisconnectedAt ?? c.ConnectedAt)
                    .Skip(5) // Keep last 5
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

        // ==================== LOAD CONVERSATION WITH PAGINATION ====================

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

        // ==================== SEND MESSAGE (TEXT ONLY) ====================

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
                // Check if users are blocked
                var isBlocked = await _messagingService.IsUserBlockedAsync(senderId, receiverId);
                if (isBlocked)
                {
                    await Clients.Caller.SendAsync("Error", "You cannot send messages to this user");
                    return;
                }

                // Verify receiver exists
                var receiver = await _userManager.FindByIdAsync(receiverId);
                if (receiver == null || !receiver.IsActive)
                {
                    await Clients.Caller.SendAsync("Error", "User not found");
                    return;
                }

                var sender = await _userManager.FindByIdAsync(senderId);
                if (sender == null)
                {
                    await Clients.Caller.SendAsync("Error", "Sender not found");
                    return;
                }

                // Parse message type
                if (!Enum.TryParse<MessageType>(messageType, true, out var msgType))
                {
                    msgType = MessageType.Text;
                }

                // Parse context type
                var messageContextType = MessageContextType.General;
                if (!string.IsNullOrEmpty(contextType))
                {
                    Enum.TryParse(contextType, true, out messageContextType);
                }

                // Create message entity
                var messageEntity = new Message
                {
                    Content = message.Trim(),
                    Type = msgType,
                    MediaUrl = null,
                    SentAt = DateTime.UtcNow,
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    ContextType = messageContextType,
                    ContextId = contextId,
                    IsRead = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Messages.AddAsync(messageEntity);
                await _context.SaveChangesAsync();

                var baseUrl = _configuration["BaseURL"];
                var contextInfo = contextId.HasValue
                    ? await _messagingService.GetMessageContextInfoAsync(messageContextType, contextId)
                    : null;

                var messageData = new
                {
                    Id = messageEntity.Id,
                    Content = messageEntity.Content,
                    Type = msgType.ToString(),
                    MediaUrl = (string)null,
                    SentAt = messageEntity.SentAt,
                    SenderId = senderId,
                    SenderName = $"{sender.FirstName} {sender.LastName}",
                    SenderProfilePicture = !string.IsNullOrEmpty(sender.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(sender.ProfilePicture, "profiles", baseUrl)
                        : null,
                    ReceiverId = receiverId,
                    ReceiverName = $"{receiver.FirstName} {receiver.LastName}",
                    ReceiverProfilePicture = !string.IsNullOrEmpty(receiver.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(receiver.ProfilePicture, "profiles", baseUrl)
                        : null,
                    IsRead = false,
                    ContextType = messageContextType.ToString(),
                    ContextId = contextId,
                    ContextInfo = contextInfo
                };

                // Send to both users
                await Clients.Caller.SendAsync("ReceivePrivateMessage", messageData);
                await Clients.User(receiverId).SendAsync("ReceivePrivateMessage", messageData);

                // Update unread count
                var unreadCount = await _messagingService.GetUnreadCountAsync(receiverId);
                await Clients.User(receiverId).SendAsync("UnreadMessagesCount", unreadCount);

                // Update conversations
                var senderConversations = await _messagingService.GetConversationsAsync(senderId);
                await Clients.User(senderId).SendAsync("ConversationsUpdated", senderConversations);

                var receiverConversations = await _messagingService.GetConversationsAsync(receiverId);
                await Clients.User(receiverId).SendAsync("ConversationsUpdated", receiverConversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        // ==================== MARK AS READ ====================

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

        // ==================== BLOCKING ====================

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

        // ==================== ONLINE STATUS ====================

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
