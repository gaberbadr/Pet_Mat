using CoreLayer.Dtos.Orders;
using CoreLayer.Service_Interface.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;
using Stripe;

namespace petmat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCouponController : BaseApiController
    {
        private readonly ICouponService _couponService;

        public AdminCouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }


        /// Get all coupons
        [HttpGet]
        [ProducesResponseType(typeof(CouponListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CouponListDto>> GetAllCoupons()
        {
            var result = await _couponService.GetAllCouponsAsync();
            return Ok(result);
        }


        /// Get coupon by ID
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CouponDto>> GetCouponById(int id)
        {
            try
            {
                var result = await _couponService.GetCouponByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Get coupon by code
        [HttpGet("code/{code}")]
        [ProducesResponseType(typeof(CouponDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CouponDto>> GetCouponByCode(string code)
        {
            try
            {
                var result = await _couponService.GetCouponByCodeAsync(code);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Create new coupon
        [HttpPost]
        [ProducesResponseType(typeof(CouponOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CouponOperationResponseDto>> CreateCoupon([FromBody] AddCouponDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _couponService.AddCouponAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Update coupon
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CouponOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CouponOperationResponseDto>> UpdateCoupon(int id, [FromBody] UpdateCouponDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _couponService.UpdateCouponAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Delete coupon
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(CouponOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CouponOperationResponseDto>> DeleteCoupon(int id)
        {
            try
            {
                var result = await _couponService.DeleteCouponAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }
    }
}
