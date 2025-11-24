using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Messag;
using CoreLayer.Enums;
using CoreLayer.Helper.Pagination;

namespace CoreLayer.Service_Interface.Messag
{
    public interface IMessagingService
    {
        // ==================== SEND MESSAGE ====================
        Task<MessageOperationResponseDto> SendMessageAsync(string senderId, SendMessageDto dto);

        // ==================== CONVERSATIONS ====================
        Task<ConversationListDto> GetConversationsAsync(string userId);

        Task<PaginationResponse<MessageResponseDto>> GetConversationMessagesAsync(
            string userId,
            string otherUserId,
            MessageFilterParams filterParams);

        // ==================== UNREAD COUNTS ====================
        Task<int> GetUnreadCountAsync(string userId);

        Task<int> GetUnreadCountFromUserAsync(string userId, string otherUserId);

        // ==================== MARK AS READ ====================
        Task<MessageOperationResponseDto> MarkAsReadAsync(int messageId, string userId);

        Task<MessageOperationResponseDto> MarkConversationAsReadAsync(
            string userId,
            string otherUserId);

        // ==================== BLOCKING ====================
        Task<BlockOperationResponseDto> BlockUserAsync(string blockerId, string blockedId);

        Task<BlockOperationResponseDto> UnblockUserAsync(string blockerId, string blockedId);

        Task<BlockedUsersListDto> GetBlockedUsersAsync(string userId);

        Task<bool> IsUserBlockedAsync(string userId1, string userId2);

        // ==================== CONTEXT INFO ====================
        Task<MessageContextInfoDto> GetMessageContextInfoAsync(
            MessageContextType contextType,
            int? contextId);
    }
}
