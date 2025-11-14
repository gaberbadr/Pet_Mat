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
    public class CartController : BaseApiController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>
        /// Get current user's cart
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var userId = GetUserId();
            var result = await _cartService.GetCartAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Add product to cart
        /// </summary>
        [HttpPost("add")]
        [ProducesResponseType(typeof(CartOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartOperationResponseDto>> AddToCart([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _cartService.AddToCartAsync(userId, dto);
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

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("item/{cartItemId}")]
        [ProducesResponseType(typeof(CartOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartOperationResponseDto>> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _cartService.UpdateCartItemAsync(userId, cartItemId, dto);
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

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("item/{cartItemId}")]
        [ProducesResponseType(typeof(CartOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartOperationResponseDto>> RemoveCartItem(int cartItemId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _cartService.RemoveCartItemAsync(userId, cartItemId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        [HttpDelete("clear")]
        [ProducesResponseType(typeof(CartOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartOperationResponseDto>> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                var result = await _cartService.ClearCartAsync(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// <summary>
        /// Apply coupon to cart
        /// </summary>
        [HttpPost("coupon")]
        [ProducesResponseType(typeof(CartOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartOperationResponseDto>> ApplyCoupon([FromBody] ApplyCouponDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _cartService.ApplyCouponAsync(userId, dto);
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

        /// <summary>
        /// Remove coupon from cart
        /// </summary>
        [HttpDelete("coupon")]
        [ProducesResponseType(typeof(CartOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartOperationResponseDto>> RemoveCoupon()
        {
            try
            {
                var userId = GetUserId();
                var result = await _cartService.RemoveCouponAsync(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }
    }

}
