using CoreLayer.Dtos.Orders;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : BaseApiController
    {
        private readonly IAdminOrderService _adminOrderService;

        public AdminOrderController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }


        /// Get all orders with pagination and optional status filter
        [HttpGet]
        [ProducesResponseType(typeof(PaginationResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginationResponse<OrderDto>>> GetAllOrders(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string status = null)
        {
            try
            {
                var result = await _adminOrderService.GetAllOrdersAsync(pageIndex, pageSize, status);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Get order by ID (admin can see any order)
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderById(int orderId)
        {
            try
            {
                var result = await _adminOrderService.GetOrderByIdAsync(orderId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Update order status
        [HttpPut("{orderId}/status")]
        [ProducesResponseType(typeof(OrderOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderOperationResponseDto>> UpdateOrderStatus(
            int orderId,
            [FromBody] UpdateOrderStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminOrderService.UpdateOrderStatusAsync(orderId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Delete order and restore product stock
        [HttpDelete("{orderId}")]
        [ProducesResponseType(typeof(OrderOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderOperationResponseDto>> DeleteOrder(int orderId)
        {
            try
            {
                var result = await _adminOrderService.DeleteOrderAsync(orderId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }
    }
}
