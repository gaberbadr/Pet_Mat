using System.Security.Claims;
using CoreLayer.Dtos.Messag;
using CoreLayer.Enums;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Messag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;
using Microsoft.AspNetCore.SignalR;
using petmat.Hubs;

namespace petmat.Controllers
{

    [Authorize]
    public class MessagingController : BaseApiController
    {
        private readonly IMessagingService _messagingService;
        private readonly ILogger<MessagingController> _logger;


        public MessagingController(
            IMessagingService messagingService,
            ILogger<MessagingController> logger)
        {
            _messagingService = messagingService;
            _logger = logger;

        }

        private string GetUserId() =>
            User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ==================== CONVERSATIONS ====================

        [HttpGet("conversations")]
        [ProducesResponseType(typeof(ConversationListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ConversationListDto>> GetConversations()
        {
            var userId = GetUserId();
            var result = await _messagingService.GetConversationsAsync(userId);
            return Ok(result);
        }

        [HttpGet("conversation/{otherUserId}")]
        [ProducesResponseType(typeof(PaginationResponse<MessageResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResponse<MessageResponseDto>>> GetConversationMessages(
            string otherUserId,
            [FromQuery] MessageFilterParams filterParams)
        {
            var userId = GetUserId();
            var result = await _messagingService.GetConversationMessagesAsync(
                userId, otherUserId, filterParams);
            return Ok(result);
        }

        // ==================== SEND MESSAGE WITH FILE ====================


        /// Send a message with optional media file (image, video, document)
        [HttpPost("send")]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100MB limit
        [ProducesResponseType(typeof(MessageOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MessageOperationResponseDto>> SendMessage([FromForm] SendMessageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();

                // The service now handles validation, DB saving, and SignalR notifications
                var result = await _messagingService.SendMessageAsync(userId, dto);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // Catches validation errors (invalid file type, empty text, etc.)
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                // Catches logic errors (blocked user, receiver not found)
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new ApiErrorResponse(500, "Failed to send message"));
            }
        }

        // ==================== UNREAD COUNTS ====================

        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = GetUserId();
            var result = await _messagingService.GetUnreadCountAsync(userId);
            return Ok(result);
        }

        [HttpGet("unread-count/{otherUserId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetUnreadCountFromUser(string otherUserId)
        {
            var userId = GetUserId();
            var result = await _messagingService.GetUnreadCountFromUserAsync(userId, otherUserId);
            return Ok(result);
        }

        // ==================== MARK AS READ ====================

        [HttpPut("message/{messageId}/read")]
        [ProducesResponseType(typeof(MessageOperationResponseDto), StatusCodes.Status200OK)]
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

        [HttpPut("conversation/{otherUserId}/read-all")]
        [ProducesResponseType(typeof(MessageOperationResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<MessageOperationResponseDto>> MarkConversationAsRead(string otherUserId)
        {
            var userId = GetUserId();
            var result = await _messagingService.MarkConversationAsReadAsync(userId, otherUserId);
            return Ok(result);
        }

        // ==================== BLOCKING ====================

        [HttpPost("block")]
        [ProducesResponseType(typeof(BlockOperationResponseDto), StatusCodes.Status200OK)]
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

        [HttpPost("unblock")]
        [ProducesResponseType(typeof(BlockOperationResponseDto), StatusCodes.Status200OK)]
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

        [HttpGet("blocked-users")]
        [ProducesResponseType(typeof(BlockedUsersListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<BlockedUsersListDto>> GetBlockedUsers()
        {
            var userId = GetUserId();
            var result = await _messagingService.GetBlockedUsersAsync(userId);
            return Ok(result);
        }

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
