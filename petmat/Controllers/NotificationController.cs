using System.Security.Claims;
using CoreLayer.Dtos;
using CoreLayer.Dtos.Messag;
using CoreLayer.Dtos.Notification;
using CoreLayer.Service_Interface.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    [Authorize]
    public class NotificationController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);


        /// Get all notifications for current user
        [HttpGet]
        [ProducesResponseType(typeof(NotificationListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<NotificationListDto>> GetNotifications()
        {
            var userId = GetUserId();
            var result = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(result);
        }


        /// Get all notifications and mark them as read automatically
        [HttpGet("read")]
        [ProducesResponseType(typeof(NotificationListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<NotificationListDto>> GetNotificationsAndMarkAsRead()
        {
            var userId = GetUserId();

            // Get notifications
            var result = await _notificationService.GetUserNotificationsAsync(userId);

            // Mark all as read automatically
            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(result);
        }


        /// Get unread count only
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var userId = GetUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(count);
        }


        /// Mark all notifications as read manually
        [HttpPut("read-all")]
        [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new SuccessResponseDto { Message = "All notifications marked as read" });
        }
    }
}
