using System.Security.Claims;
using CoreLayer.Dtos.Orders;
using CoreLayer.Service_Interface.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    [Authorize]
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IDeliveryMethodService _deliveryMethodService;

        public OrderController(IOrderService orderService, IDeliveryMethodService deliveryMethodService)
        {
            _orderService = orderService;
            _deliveryMethodService = deliveryMethodService;
        }

        private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        private string GetUserEmail() => User.FindFirstValue(ClaimTypes.Email);

        /// Create order (Works for both Online and Cash on Delivery)
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var email = GetUserEmail();
                var result = await _orderService.CreateOrderAsync(userId, email, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        /// Get order by ID
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderById(int orderId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _orderService.GetOrderByIdAsync(userId, orderId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// Get all orders for current user
        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(OrderListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderListDto>> GetMyOrders()
        {
            var userId = GetUserId();
            var result = await _orderService.GetUserOrdersAsync(userId);
            return Ok(result);
        }

        #region
        /*
        User clicks “Checkout / Pay”.

        Frontend calls: POST /api/order

        The server:

        Creates the order.

        Creates the Stripe PaymentIntent.

        Returns:

        clientSecret

        paymentIntentId

        Before the frontend calls stripe.confirmCardPayment(clientSecret):
        The frontend calls:
        GET /api/order/validate-payment/{paymentIntentId}

        If the validation returns OK →
        The frontend proceeds with confirming the payment.

        Stripe webhook then updates the order status after the payment is completed.

        Why is this useful?

        It prevents the client from using an old clientSecret or a PaymentIntent that doesn’t belong to them.
         */
        #endregion
        /// Validate if order exists for payment intent (called before completing payment)
        [HttpGet("validate-payment/{paymentIntentId}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> ValidateOrderForPayment(string paymentIntentId)
        {
            try
            {
                var orderExists = await _orderService.ValidateOrderExistsForPaymentAsync(paymentIntentId);

                if (!orderExists)
                {
                    return NotFound(new ApiErrorResponse(404, "No order found for this payment intent. Please create your order first."));
                }

                return Ok(new { exists = true, message = "Order found, you can proceed with payment" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        /// Cancel order (user can cancel own order if still Pending or PendingPayment)
        [HttpPost("{orderId}/cancel")]
        [ProducesResponseType(typeof(OrderOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderOperationResponseDto>> CancelOrder(int orderId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _orderService.CancelOrderAsync(userId, orderId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        /// Get all delivery methods
        [HttpGet("delivery-methods")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DeliveryMethodListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<DeliveryMethodListDto>> GetDeliveryMethods()
        {
            var result = await _deliveryMethodService.GetAllDeliveryMethodsAsync();
            return Ok(result);
        }
    }
}
