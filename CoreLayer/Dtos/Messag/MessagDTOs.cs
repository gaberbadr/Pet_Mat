using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Enums;

namespace CoreLayer.Dtos.Messag
{
    // Request DTOs
    public class MessageFilterParams
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class BlockUserDto
    {
        [Required]
        public string BlockedUserId { get; set; }
    }

    public class SendNotificationDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Message { get; set; }
    }

    // Response DTOs
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        // Sender info
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderProfilePicture { get; set; }

        // Receiver info
        public string ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverProfilePicture { get; set; }

        // Context info
        public MessageContextType ContextType { get; set; }
        public int? ContextId { get; set; }
        public MessageContextInfoDto ContextInfo { get; set; }
    }

    public class MessageContextInfoDto
    {
        public MessageContextType Type { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ConversationDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
        public MessageResponseDto LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ConversationListDto
    {
        public int Count { get; set; }
        public IEnumerable<ConversationDto> Data { get; set; }
    }

    public class BlockedUserDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
        public DateTime BlockedAt { get; set; }
    }

    public class BlockedUsersListDto
    {
        public int Count { get; set; }
        public IEnumerable<BlockedUserDto> Data { get; set; }
    }

    public class MessageOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? MessageId { get; set; }
    }

    public class BlockOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ConversationUserInfoDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
    }
}
