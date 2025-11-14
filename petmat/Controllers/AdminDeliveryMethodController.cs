using CoreLayer.Dtos.Orders;
using CoreLayer.Service_Interface.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDeliveryMethodController : BaseApiController
    {
        private readonly IDeliveryMethodService _deliveryMethodService;

        public AdminDeliveryMethodController(IDeliveryMethodService deliveryMethodService)
        {
            _deliveryMethodService = deliveryMethodService;
        }


        /// Get all delivery methods
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DeliveryMethodListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<DeliveryMethodListDto>> GetAllDeliveryMethods()
        {
            var result = await _deliveryMethodService.GetAllDeliveryMethodsAsync();
            return Ok(result);
        }


        /// Get delivery method by ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DeliveryMethodDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeliveryMethodDto>> GetDeliveryMethodById(int id)
        {
            try
            {
                var result = await _deliveryMethodService.GetDeliveryMethodByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Create new delivery method
        [HttpPost]
        [ProducesResponseType(typeof(DeliveryMethodOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DeliveryMethodOperationResponseDto>> CreateDeliveryMethod([FromBody] AddDeliveryMethodDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            var result = await _deliveryMethodService.AddDeliveryMethodAsync(dto);
            return Ok(result);
        }


        /// Update delivery method
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DeliveryMethodOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeliveryMethodOperationResponseDto>> UpdateDeliveryMethod(int id, [FromBody] UpdateDeliveryMethodDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _deliveryMethodService.UpdateDeliveryMethodAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// <summary>
        /// Delete delivery method
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(DeliveryMethodOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeliveryMethodOperationResponseDto>> DeleteDeliveryMethod(int id)
        {
            try
            {
                var result = await _deliveryMethodService.DeleteDeliveryMethodAsync(id);
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
    }
}
