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
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Messag
{
    public class MessagingService : IMessagingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public MessagingService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        // ==================== CONVERSATIONS ====================

        public async Task<ConversationListDto> GetConversationsAsync(string userId)
        {
            var baseUrl = _configuration["BaseURL"];

            // Get all messages for the user
            var spec = new LatestMessagePerConversationSpecification(userId);
            var allMessages = await _unitOfWork.Repository<Message, int>()
                .GetAllWithSpecficationAsync(spec);

            // Group by conversation partner and get latest message + unread count
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

                // Check if user is online (you can implement this with SignalR tracking)
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

            // Get paginated messages
            var spec = new MessagesBetweenUsersPaginatedSpecification(userId, otherUserId, filterParams);
            var messages = await _unitOfWork.Repository<Message, int>()
                .GetAllWithSpecficationAsync(spec);

            // Get total count
            var countSpec = new MessagesBetweenUsersCountSpecification(userId, otherUserId);
            var totalCount = await _unitOfWork.Repository<Message, int>().GetCountAsync(countSpec);

            // Map to DTOs
            var messageDtos = new List<MessageResponseDto>();
            foreach (var message in messages)
            {
                var messageDto = MapToMessageDto(message, baseUrl);

                // Add context info if exists
                if (message.ContextId.HasValue)
                {
                    messageDto.ContextInfo = await GetMessageContextInfoAsync(
                        message.ContextType,
                        message.ContextId);
                }

                messageDtos.Add(messageDto);
            }

            // Reverse to show oldest first (messages are fetched newest first for pagination)
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
            if (!contextId.HasValue)
                return null;

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

                    // Get animal details
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
            // Check if user has any active connections
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
