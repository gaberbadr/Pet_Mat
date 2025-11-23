using System;
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


        private static readonly Dictionary<string, HashSet<string>> _onlineUsers = new();
        private static readonly object _lock = new object();

        public ChatHub(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMessagingService messagingService,
            INotificationService notificationService,
            IConfiguration configuration, ILogger<ChatHub> logger)
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
            if (userId != null)
            {
                var connection = new UserConnection
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = userId,
                    ConnectedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.UserConnections.AddAsync(connection);
                await _context.SaveChangesAsync();

                lock (_lock)
                {
                    if (!_onlineUsers.ContainsKey(userId))
                    {
                        _onlineUsers[userId] = new HashSet<string>();
                    }
                    _onlineUsers[userId].Add(Context.ConnectionId);
                }

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
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;

            var connection = await _context.UserConnections
                .FirstOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);

            if (connection != null)
            {
                connection.IsActive = false;
                connection.DisconnectedAt = DateTime.UtcNow;
                _context.UserConnections.Update(connection);
                await _context.SaveChangesAsync();
            }

            if (userId != null)
            {
                bool isStillOnline = false;

                lock (_lock)
                {
                    if (_onlineUsers.ContainsKey(userId))
                    {
                        _onlineUsers[userId].Remove(Context.ConnectionId);

                        if (_onlineUsers[userId].Count == 0)
                        {
                            _onlineUsers.Remove(userId);
                        }
                        else
                        {
                            isStillOnline = true;
                        }
                    }
                }

                if (!isStillOnline)
                {
                    await Clients.All.SendAsync("UserOnlineStatus", new
                    {
                        UserId = userId,
                        IsOnline = false,
                        LastSeen = DateTime.UtcNow
                    });
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ==================== LOAD CONVERSATION WITH PAGINATION ====================


        /// Load conversation messages with pagination
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
                // Check if other user exists
                var otherUser = await _userManager.FindByIdAsync(otherUserId);
                if (otherUser == null || !otherUser.IsActive)
                {
                    await Clients.Caller.SendAsync("Error", "User not found or inactive");
                    await Clients.Caller.SendAsync("ConversationLoaded", new
                    {
                        OtherUserId = otherUserId,
                        Messages = new List<object>(),
                        TotalCount = 0,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        HasMore = false
                    });
                    return;
                }

                var filterParams = new MessageFilterParams
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                var messages = await _messagingService.GetConversationMessagesAsync(
                    userId,
                    otherUserId,
                    filterParams);

                await Clients.Caller.SendAsync("ConversationLoaded", new
                {
                    OtherUserId = otherUserId,
                    Messages = messages.Data,
                    TotalCount = messages.Count,
                    PageIndex = messages.PageIndex,
                    PageSize = messages.PageSize,
                    HasMore = (messages.PageIndex * messages.PageSize) < messages.Count
                });

                _logger.LogInformation($"Conversation loaded: {userId} with {otherUserId}, {messages.Data.Count()} messages");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading conversation between {userId} and {otherUserId}");
                await Clients.Caller.SendAsync("Error", "Failed to load conversation: " + ex.Message);

                // Send empty response so UI doesn't hang
                await Clients.Caller.SendAsync("ConversationLoaded", new
                {
                    OtherUserId = otherUserId,
                    Messages = new List<object>(),
                    TotalCount = 0,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    HasMore = false
                });
            }
        }

        // ==================== SEND MESSAGE ====================

        public async Task SendPrivateMessage(
            string receiverId,
            string message,
            string contextType = null,
            int? contextId = null)
        {
            var senderId = Context.UserIdentifier;

            _logger.LogInformation($"SendPrivateMessage called: Sender={senderId}, Receiver={receiverId}, Message={message}");

            if (string.IsNullOrEmpty(senderId) || string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Invalid message data");
                await Clients.Caller.SendAsync("Error", "Invalid message data");
                return;
            }

            try
            {
                // Check if users are blocked
                _logger.LogInformation("Checking if users are blocked...");
                var isBlocked = await _messagingService.IsUserBlockedAsync(senderId, receiverId);
                if (isBlocked)
                {
                    _logger.LogWarning($"Users are blocked: {senderId} <-> {receiverId}");
                    await Clients.Caller.SendAsync("Error", "You cannot send messages to this user");
                    return;
                }

                // Verify receiver exists
                _logger.LogInformation($"Finding receiver: {receiverId}");
                var receiver = await _userManager.FindByIdAsync(receiverId);
                if (receiver == null || !receiver.IsActive)
                {
                    _logger.LogWarning($"Receiver not found or inactive: {receiverId}");
                    await Clients.Caller.SendAsync("Error", "User not found");
                    return;
                }

                // Get sender info
                _logger.LogInformation($"Finding sender: {senderId}");
                var sender = await _userManager.FindByIdAsync(senderId);
                if (sender == null)
                {
                    _logger.LogWarning($"Sender not found: {senderId}");
                    await Clients.Caller.SendAsync("Error", "Sender not found");
                    return;
                }

                // Parse context type
                MessageContextType messageContextType = MessageContextType.General;
                if (!string.IsNullOrEmpty(contextType))
                {
                    Enum.TryParse(contextType, true, out messageContextType);
                }

                _logger.LogInformation("Creating message entity...");

                // Create message entity with ALL required fields
                var messageEntity = new Message
                {
                    Content = message.Trim(),
                    Type = MessageType.Text,
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

                _logger.LogInformation($"Message entity created: {System.Text.Json.JsonSerializer.Serialize(new
                {
                    messageEntity.Content,
                    messageEntity.SenderId,
                    messageEntity.ReceiverId,
                    messageEntity.Type,
                    messageEntity.ContextType
                })}");

                // Save to database
                _logger.LogInformation("Adding message to context...");
                await _context.Messages.AddAsync(messageEntity);

                _logger.LogInformation("Saving changes to database...");
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Message saved successfully with ID: {messageEntity.Id}");

                // Get base URL for file URLs
                var baseUrl = _configuration["BaseURL"];

                // Get context info if exists
                var contextInfo = contextId.HasValue
                    ? await _messagingService.GetMessageContextInfoAsync(messageContextType, contextId)
                    : null;

                // Create response DTO
                var messageData = new
                {
                    Id = messageEntity.Id,
                    Content = messageEntity.Content,
                    Type = MessageType.Text.ToString(),
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

                _logger.LogInformation("Sending message to clients...");

                // Send to sender
                await Clients.Caller.SendAsync("ReceivePrivateMessage", messageData);

                // Send to receiver
                await Clients.User(receiverId).SendAsync("ReceivePrivateMessage", messageData);

                // Update unread count for receiver
                var unreadCount = await _messagingService.GetUnreadCountAsync(receiverId);
                await Clients.User(receiverId).SendAsync("UnreadMessagesCount", unreadCount);

                // Update conversations for both users
                var senderConversations = await _messagingService.GetConversationsAsync(senderId);
                await Clients.User(senderId).SendAsync("ConversationsUpdated", senderConversations);

                var receiverConversations = await _messagingService.GetConversationsAsync(receiverId);
                await Clients.User(receiverId).SendAsync("ConversationsUpdated", receiverConversations);

                _logger.LogInformation($"✅ Message sent successfully: {senderId} -> {receiverId}");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "❌ DATABASE ERROR sending message");
                _logger.LogError($"Inner Exception: {dbEx.InnerException?.Message}");
                _logger.LogError($"Stack Trace: {dbEx.StackTrace}");
                await Clients.Caller.SendAsync("Error", $"Failed to save message: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ GENERAL ERROR sending private message");
                _logger.LogError($"Exception Type: {ex.GetType().Name}");
                _logger.LogError($"Message: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
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

                    // Update conversations
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

                // Update conversations
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

                // Update conversations
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

                // Update conversations
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
            bool isOnline;
            DateTime? lastSeen = null;

            lock (_lock)
            {
                isOnline = _onlineUsers.ContainsKey(userId) && _onlineUsers[userId].Count > 0;
            }

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
