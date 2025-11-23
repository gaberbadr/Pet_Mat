using System.Security.Claims;
using CoreLayer.Dtos.Messag;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Messag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{

    [Authorize]
    public class MessagingController : BaseApiController
    {
        private readonly IMessagingService _messagingService;

        public MessagingController(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        private string GetUserId() =>
            User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ==================== CONVERSATIONS ====================

        /// <summary>
        /// Get all conversations grouped by user with last message and unread count
        /// </summary>
        [HttpGet("conversations")]
        [ProducesResponseType(typeof(ConversationListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ConversationListDto>> GetConversations()
        {
            var userId = GetUserId();
            var result = await _messagingService.GetConversationsAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Get paginated conversation messages with a specific user (20 messages per page)
        /// </summary>
        [HttpGet("conversation/{otherUserId}")]
        [ProducesResponseType(typeof(PaginationResponse<MessageResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResponse<MessageResponseDto>>> GetConversationMessages(
            string otherUserId,
            [FromQuery] MessageFilterParams filterParams)
        {
            var userId = GetUserId();
            var result = await _messagingService.GetConversationMessagesAsync(
                userId,
                otherUserId,
                filterParams);
            return Ok(result);
        }

        /// <summary>
        /// Get total unread messages count
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = GetUserId();
            var result = await _messagingService.GetUnreadCountAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Get unread messages count from specific user
        /// </summary>
        [HttpGet("unread-count/{otherUserId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetUnreadCountFromUser(string otherUserId)
        {
            var userId = GetUserId();
            var result = await _messagingService.GetUnreadCountFromUserAsync(userId, otherUserId);
            return Ok(result);
        }

        /// <summary>
        /// Mark single message as read
        /// </summary>
        [HttpPut("message/{messageId}/read")]
        [ProducesResponseType(typeof(MessageOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MessageOperationResponseDto>> MarkMessageAsRead(int messageId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _messagingService.MarkAsReadAsync(messageId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiErrorResponse(403, ex.Message));
            }
        }

        /// <summary>
        /// Mark all messages in a conversation as read
        /// </summary>
        [HttpPut("conversation/{otherUserId}/read-all")]
        [ProducesResponseType(typeof(MessageOperationResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<MessageOperationResponseDto>> MarkConversationAsRead(string otherUserId)
        {
            var userId = GetUserId();
            var result = await _messagingService.MarkConversationAsReadAsync(userId, otherUserId);
            return Ok(result);
        }

        // ==================== BLOCKING ====================

        /// <summary>
        /// Block a user
        /// </summary>
        [HttpPost("block")]
        [ProducesResponseType(typeof(BlockOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BlockOperationResponseDto>> BlockUser([FromBody] BlockUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _messagingService.BlockUserAsync(userId, dto.BlockedUserId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Unblock a user
        /// </summary>
        [HttpPost("unblock")]
        [ProducesResponseType(typeof(BlockOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlockOperationResponseDto>> UnblockUser([FromBody] BlockUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _messagingService.UnblockUserAsync(userId, dto.BlockedUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// <summary>
        /// Get list of blocked users
        /// </summary>
        [HttpGet("blocked-users")]
        [ProducesResponseType(typeof(BlockedUsersListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<BlockedUsersListDto>> GetBlockedUsers()
        {
            var userId = GetUserId();
            var result = await _messagingService.GetBlockedUsersAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Check if a user is blocked
        /// </summary>
        [HttpGet("is-blocked/{otherUserId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> IsUserBlocked(string otherUserId)
        {
            var userId = GetUserId();
            var result = await _messagingService.IsUserBlockedAsync(userId, otherUserId);
            return Ok(result);
        }
    }
}
