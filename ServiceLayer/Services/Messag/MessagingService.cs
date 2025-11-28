using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Messag;
using CoreLayer.Entities.Accessories;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Community;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Messages;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Enums;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Messag;
using CoreLayer.Specifications;
using CoreLayer.Specifications.Messag;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using petmat.Hubs;

namespace ServiceLayer.Services.Messag
{
    public class MessagingService : IMessagingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessagingService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagingService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<MessagingService> logger,
            IHubContext<ChatHub> hubContext,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // ==================== SEND MESSAGE WITH FILE ====================

        public async Task<MessageOperationResponseDto> SendMessageAsync(string senderId, SendMessageDto dto)
        {
            // 1. Validate Content
            ValidateMessageContent(dto);

            // 2. Validate Receiver (We fetch the entity here)
            var receiver = await _userManager.FindByIdAsync(dto.ReceiverId);
            if (receiver == null || !receiver.IsActive)
                throw new InvalidOperationException("Receiver not found or inactive");

            // 3. Fetch Sender
            var sender = await _userManager.FindByIdAsync(senderId);

            // 4. Check Blocking
            if (await IsUserBlockedAsync(senderId, dto.ReceiverId))
                throw new InvalidOperationException("Cannot send messages to this blocked user");

            // 5. Handle File Upload
            string mediaUrl = null;
            if (dto.MediaFile != null)
            {
                var folderName = GetFolderNameForMessageType(dto.Type);
                mediaUrl = DocumentSetting.Upload(dto.MediaFile, folderName);
            }

            // 6. Create Entity
            var message = new Message
            {
                Content = dto.Content?.Trim() ?? string.Empty,
                Type = dto.Type,
                MediaUrl = mediaUrl,
                SentAt = DateTime.UtcNow,
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                ContextType = dto.ContextType,
                ContextId = dto.ContextId,
                IsRead = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Message, int>().AddAsync(message);
            await _unitOfWork.CompleteAsync();


            message.Sender = sender;
            message.Receiver = receiver;

            // 7. Map 
            var baseUrl = _configuration["BaseURL"];
            var messageDto = MapToMessageDto(message, baseUrl);

            // 8. Add Context Info
            if (message.ContextId.HasValue)
            {
                messageDto.ContextInfo = await GetMessageContextInfoAsync(message.ContextType, message.ContextId);
            }

            // 9. Broadcast
            await BroadcastMessageAsync(senderId, dto.ReceiverId, messageDto);

            return new MessageOperationResponseDto
            {
                Success = true,
                Message = "Message sent successfully",
                MessageId = message.Id,
                MessageData = messageDto
            };
        }

        // ==================== PRIVATE HELPER METHODS ====================

        private async Task BroadcastMessageAsync(string senderId, string receiverId, MessageResponseDto messageData)
        {
            // Send to sender (for immediate UI update on multiple devices)
            await _hubContext.Clients.User(senderId).SendAsync("ReceivePrivateMessage", messageData);

            // Send to receiver
            await _hubContext.Clients.User(receiverId).SendAsync("ReceivePrivateMessage", messageData);

            // Update Receiver's unread count
            var unreadCount = await GetUnreadCountAsync(receiverId);
            await _hubContext.Clients.User(receiverId).SendAsync("UnreadMessagesCount", unreadCount);

            // Refresh conversation lists for both
            var senderConversations = await GetConversationsAsync(senderId);
            await _hubContext.Clients.User(senderId).SendAsync("ConversationsUpdated", senderConversations);

            var receiverConversations = await GetConversationsAsync(receiverId);
            await _hubContext.Clients.User(receiverId).SendAsync("ConversationsUpdated", receiverConversations);
        }

        private void ValidateMessageContent(SendMessageDto dto)
        {
            // Validate text content
            if (dto.Type == MessageType.Text && string.IsNullOrWhiteSpace(dto.Content))
            {
                throw new ArgumentException("Text messages must have content");
            }

            // Validate media presence
            if (dto.Type != MessageType.Text && dto.MediaFile == null)
            {
                throw new ArgumentException($"{dto.Type} messages must include a media file");
            }

            // Validate file extension
            if (dto.MediaFile != null)
            {
                var extension = Path.GetExtension(dto.MediaFile.FileName).ToLowerInvariant();
                string error = dto.Type switch
                {
                    MessageType.Image => !new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(extension)
                        ? "Invalid image file. Allowed: .jpg, .jpeg, .png, .gif, .webp" : null,

                    MessageType.Video => !new[] { ".mp4", ".webm", ".ogg", ".mov", ".avi", ".mkv" }.Contains(extension)
                        ? "Invalid video file. Allowed: .mp4, .webm, .ogg, .mov, .avi, .mkv" : null,

                    MessageType.Document => !new[] { ".pdf", ".docx", ".xlsx", ".pptx", ".txt", ".rtf" }.Contains(extension)
                        ? "Invalid document file. Allowed: .pdf, .docx, .xlsx, .pptx, .txt, .rtf" : null,

                    _ => null
                };

                if (error != null) throw new ArgumentException(error);
            }
        }

        private string GetFolderNameForMessageType(MessageType type)
        {
            return type switch
            {
                MessageType.Image => "messages/photos",
                MessageType.Video => "messages/videos",
                MessageType.Document => "messages/documents",
                _ => "messages"
            };
        }

        // ==================== CONVERSATIONS ====================

        public async Task<ConversationListDto> GetConversationsAsync(string userId)
        {
            var baseUrl = _configuration["BaseURL"];

            var spec = new LatestMessagePerConversationSpecification(userId);
            var allMessages = await _unitOfWork.Repository<Message, int>()
                .GetAllWithSpecficationAsync(spec);

            var conversationGroups = allMessages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LatestMessage = g.OrderByDescending(m => m.SentAt).First(),
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                })
                .OrderByDescending(c => c.LatestMessage.SentAt)
                .ToList();

            var conversations = new List<ConversationDto>();

            foreach (var group in conversationGroups)
            {
                var otherUser = group.LatestMessage.SenderId == userId
                    ? group.LatestMessage.Receiver
                    : group.LatestMessage.Sender;

                var isOnline = await CheckUserOnlineStatus(group.OtherUserId);
                var lastSeen = await GetUserLastSeen(group.OtherUserId);

                conversations.Add(new ConversationDto
                {
                    UserId = group.OtherUserId,
                    UserName = $"{otherUser.FirstName} {otherUser.LastName}",
                    UserProfilePicture = !string.IsNullOrEmpty(otherUser.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(otherUser.ProfilePicture, "profiles", baseUrl)
                        : null,
                    IsOnline = isOnline,
                    LastSeen = lastSeen,
                    LastMessage = MapToMessageDto(group.LatestMessage, baseUrl),
                    UnreadCount = group.UnreadCount
                });
            }

            return new ConversationListDto
            {
                Count = conversations.Count,
                Data = conversations
            };
        }

        // ==================== MESSAGES WITH PAGINATION ====================

        public async Task<PaginationResponse<MessageResponseDto>> GetConversationMessagesAsync(
            string userId,
            string otherUserId,
            MessageFilterParams filterParams)
        {
            var baseUrl = _configuration["BaseURL"];

            var spec = new MessagesBetweenUsersPaginatedSpecification(userId, otherUserId, filterParams);
            var messages = await _unitOfWork.Repository<Message, int>()
                .GetAllWithSpecficationAsync(spec);

            var countSpec = new MessagesBetweenUsersCountSpecification(userId, otherUserId);
            var totalCount = await _unitOfWork.Repository<Message, int>().GetCountAsync(countSpec);

            var messageDtos = new List<MessageResponseDto>();
            foreach (var message in messages)
            {
                var messageDto = MapToMessageDto(message, baseUrl);

                if (message.ContextId.HasValue)
                {
                    messageDto.ContextInfo = await GetMessageContextInfoAsync(
                        message.ContextType, message.ContextId);
                }

                messageDtos.Add(messageDto);
            }

            messageDtos.Reverse();

            return new PaginationResponse<MessageResponseDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                messageDtos
            );
        }

        // ==================== UNREAD COUNTS ====================

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var spec = new UnreadMessagesCountSpecification(userId);
            return await _unitOfWork.Repository<Message, int>().GetCountAsync(spec);
        }

        public async Task<int> GetUnreadCountFromUserAsync(string userId, string otherUserId)
        {
            var spec = new UnreadMessagesFromUserSpecification(userId, otherUserId);
            return await _unitOfWork.Repository<Message, int>().GetCountAsync(spec);
        }

        // ==================== MARK AS READ ====================

        public async Task<MessageOperationResponseDto> MarkAsReadAsync(int messageId, string userId)
        {
            var message = await _unitOfWork.Repository<Message, int>().GetAsync(messageId);

            if (message == null)
                throw new KeyNotFoundException("Message not found");

            if (message.ReceiverId != userId)
                throw new UnauthorizedAccessException("You can only mark your own messages as read");

            if (message.IsRead)
            {
                return new MessageOperationResponseDto
                {
                    Success = true,
                    Message = "Message already read",
                    MessageId = messageId
                };
            }

            message.IsRead = true;
            _unitOfWork.Repository<Message, int>().Update(message);
            await _unitOfWork.CompleteAsync();

            return new MessageOperationResponseDto
            {
                Success = true,
                Message = "Message marked as read",
                MessageId = messageId
            };
        }

        public async Task<MessageOperationResponseDto> MarkConversationAsReadAsync(
            string userId,
            string otherUserId)
        {
            var spec = new UnreadMessagesFromUserSpecification(userId, otherUserId);
            var unreadMessages = await _unitOfWork.Repository<Message, int>()
                .GetAllWithSpecficationAsync(spec);

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                _unitOfWork.Repository<Message, int>().Update(message);
            }

            await _unitOfWork.CompleteAsync();

            return new MessageOperationResponseDto
            {
                Success = true,
                Message = $"Marked {unreadMessages.Count()} messages as read"
            };
        }

        // ==================== BLOCKING ====================

        public async Task<BlockOperationResponseDto> BlockUserAsync(string blockerId, string blockedId)
        {
            if (blockerId == blockedId)
                throw new InvalidOperationException("You cannot block yourself");

            var spec = new ActiveBlockSpecification(blockerId, blockedId);
            var existingBlock = await _unitOfWork.Repository<UserBlock, int>()
                .GetWithSpecficationAsync(spec);

            if (existingBlock != null)
                throw new InvalidOperationException("User is already blocked");

            var block = new UserBlock
            {
                BlockerId = blockerId,
                BlockedId = blockedId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<UserBlock, int>().AddAsync(block);
            await _unitOfWork.CompleteAsync();

            return new BlockOperationResponseDto
            {
                Success = true,
                Message = "User blocked successfully"
            };
        }

        public async Task<BlockOperationResponseDto> UnblockUserAsync(string blockerId, string blockedId)
        {
            var spec = new ActiveBlockSpecification(blockerId, blockedId);
            var block = await _unitOfWork.Repository<UserBlock, int>()
                .GetWithSpecficationAsync(spec);

            if (block == null)
                throw new KeyNotFoundException("Block not found");

            block.IsActive = false;
            block.UnblockedAt = DateTime.UtcNow;
            _unitOfWork.Repository<UserBlock, int>().Update(block);
            await _unitOfWork.CompleteAsync();

            return new BlockOperationResponseDto
            {
                Success = true,
                Message = "User unblocked successfully"
            };
        }

        public async Task<BlockedUsersListDto> GetBlockedUsersAsync(string userId)
        {
            var baseUrl = _configuration["BaseURL"];
            var spec = new BlockedUsersSpecification(userId);
            var blocks = await _unitOfWork.Repository<UserBlock, int>()
                .GetAllWithSpecficationAsync(spec);

            var blockedUsers = blocks.Select(b => new BlockedUserDto
            {
                UserId = b.BlockedId,
                UserName = $"{b.Blocked.FirstName} {b.Blocked.LastName}",
                UserProfilePicture = !string.IsNullOrEmpty(b.Blocked.ProfilePicture)
                    ? DocumentSetting.GetFileUrl(b.Blocked.ProfilePicture, "profiles", baseUrl)
                    : null,
                BlockedAt = b.CreatedAt
            }).ToList();

            return new BlockedUsersListDto
            {
                Count = blockedUsers.Count,
                Data = blockedUsers
            };
        }

        public async Task<bool> IsUserBlockedAsync(string userId1, string userId2)
        {
            var spec = new IsUserBlockedSpecification(userId1, userId2);
            var block = await _unitOfWork.Repository<UserBlock, int>()
                .GetWithSpecficationAsync(spec);

            return block != null;
        }

        // ==================== CONTEXT INFO ====================

        public async Task<MessageContextInfoDto> GetMessageContextInfoAsync(
            MessageContextType contextType,
            int? contextId)
        {
            if (!contextId.HasValue) return null;

            var baseUrl = _configuration["BaseURL"];

            switch (contextType)
            {
                case MessageContextType.Post:
                    var post = await _unitOfWork.Repository<Post, int>().GetAsync(contextId.Value);
                    if (post == null) return null;

                    var firstImage = !string.IsNullOrEmpty(post.ImageUrls)
                        ? post.ImageUrls.Split(',').FirstOrDefault()
                        : null;

                    return new MessageContextInfoDto
                    {
                        Type = MessageContextType.Post,
                        Id = post.Id,
                        Title = post.Content.Length > 50
                            ? post.Content.Substring(0, 50) + "..."
                            : post.Content,
                        ImageUrl = !string.IsNullOrEmpty(firstImage)
                            ? DocumentSetting.GetFileUrl(firstImage, "posts", baseUrl)
                            : null
                    };

                case MessageContextType.AnimalListing:
                    var listing = await _unitOfWork.Repository<AnimalListing, int>()
                        .GetAsync(contextId.Value);
                    if (listing == null) return null;

                    var animal = await _unitOfWork.Repository<Animal, int>()
                        .GetAsync(listing.AnimalId);

                    var animalImage = !string.IsNullOrEmpty(animal?.ImageUrl)
                        ? animal.ImageUrl.Split(',').FirstOrDefault()
                        : null;

                    return new MessageContextInfoDto
                    {
                        Type = MessageContextType.AnimalListing,
                        Id = listing.Id,
                        Title = listing.Title,
                        ImageUrl = !string.IsNullOrEmpty(animalImage)
                            ? DocumentSetting.GetFileUrl(animalImage, "animals", baseUrl)
                            : null
                    };

                case MessageContextType.AccessoryListing:
                    var accessory = await _unitOfWork.Repository<AccessoryListing, int>()
                        .GetAsync(contextId.Value);
                    if (accessory == null) return null;

                    var accessoryImage = !string.IsNullOrEmpty(accessory.ImageUrls)
                        ? accessory.ImageUrls.Split(',').FirstOrDefault()
                        : null;

                    return new MessageContextInfoDto
                    {
                        Type = MessageContextType.AccessoryListing,
                        Id = accessory.Id,
                        Title = accessory.Title,
                        ImageUrl = !string.IsNullOrEmpty(accessoryImage)
                            ? DocumentSetting.GetFileUrl(accessoryImage, "accessories", baseUrl)
                            : null
                    };

                case MessageContextType.PharmacyStore:
                    var pharmacy = await _unitOfWork.Repository<PharmacyListing, int>()
                        .GetAsync(contextId.Value);
                    if (pharmacy == null) return null;

                    var pharmacyImage = !string.IsNullOrEmpty(pharmacy.ImageUrls)
                        ? pharmacy.ImageUrls.Split(',').FirstOrDefault()
                        : null;

                    return new MessageContextInfoDto
                    {
                        Type = MessageContextType.PharmacyStore,
                        Id = pharmacy.Id,
                        Title = pharmacy.Title,
                        ImageUrl = !string.IsNullOrEmpty(pharmacyImage)
                            ? DocumentSetting.GetFileUrl(pharmacyImage, "pharmacy", baseUrl)
                            : null
                    };

                default:
                    return null;
            }
        }

        // ==================== HELPER METHODS ====================

        private MessageResponseDto MapToMessageDto(Message message, string baseUrl)
        {
            return new MessageResponseDto
            {
                Id = message.Id,
                Content = message.Content,
                Type = message.Type,
                MediaUrl = !string.IsNullOrEmpty(message.MediaUrl)
                    ? DocumentSetting.GetFileUrl(
                        message.MediaUrl,
                        GetFolderNameForMessageType(message.Type),
                        baseUrl)
                    : null,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}",
                SenderProfilePicture = !string.IsNullOrEmpty(message.Sender.ProfilePicture)
                    ? DocumentSetting.GetFileUrl(message.Sender.ProfilePicture, "profiles", baseUrl)
                    : null,
                ReceiverId = message.ReceiverId,
                ReceiverName = $"{message.Receiver.FirstName} {message.Receiver.LastName}",
                ReceiverProfilePicture = !string.IsNullOrEmpty(message.Receiver.ProfilePicture)
                    ? DocumentSetting.GetFileUrl(message.Receiver.ProfilePicture, "profiles", baseUrl)
                    : null,
                ContextType = message.ContextType,
                ContextId = message.ContextId
            };
        }

        private async Task<bool> CheckUserOnlineStatus(string userId)
        {
            var connections = await _unitOfWork.Repository<UserConnection, int>()
                .FindAsync(c => c.UserId == userId && c.IsActive);

            return connections.Any();
        }

        private async Task<DateTime?> GetUserLastSeen(string userId)
        {
            var connections = await _unitOfWork.Repository<UserConnection, int>()
                .FindAsync(c => c.UserId == userId);

            var lastConnection = connections
                .OrderByDescending(c => c.DisconnectedAt ?? c.ConnectedAt)
                .FirstOrDefault();

            return lastConnection?.DisconnectedAt ?? lastConnection?.ConnectedAt;
        }
    }
}
