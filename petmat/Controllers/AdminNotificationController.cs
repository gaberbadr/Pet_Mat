using CoreLayer.Dtos;
using CoreLayer.Dtos.Messag;
using CoreLayer.Service_Interface.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminNotificationController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public AdminNotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        /// Send notification to a specific user (Admin only)
        [HttpPost("send")]
        [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SendNotification([FromBody] SendNotificationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                await _notificationService.AddNotificationAsync(dto.UserId, dto.Message);
                return Ok(new SuccessResponseDto { Message = "Notification sent successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Failed to send notification: {ex.Message}"));
            }
        }
    }
}


