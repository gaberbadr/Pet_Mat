using System.Security.Claims;
using CoreLayer.Dtos.Accessory;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Accessory;
using CoreLayer.Service_Interface.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{

    public class AccessoryController : BaseApiController
    {

        private readonly IUserAccessoryManagement _userAccessoryManagement;

        public AccessoryController(IUserAccessoryManagement userAccessoryManagement)
        {
           
            _userAccessoryManagement = userAccessoryManagement;
        }

        private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);


        // ==================== ACCESSORY LISTINGS ====================


        /// Get all accessory listings with filters and pagination
        [ProducesResponseType(typeof(PaginationResponse<AccessoryListingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpGet("accessory-listings")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginationResponse<AccessoryListingResponseDto>>> GetAllAccessoryListings([FromQuery] AccessoryListingFilterParams filterParams)
        {
            try
            {
                var result = await _userAccessoryManagement.GetAllAccessoryListingsAsync(filterParams);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
            catch (Exception ex)
            {


                return StatusCode(500, new ApiErrorResponse(500, "An error occurred while fetching accessory listings"));
            }
        }

        /// Get accessory listing by ID
        [ProducesResponseType(typeof(AccessoryListingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("accessory-listing/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AccessoryListingResponseDto>> GetAccessoryListingById(int id)
        {
            try
            {
                var result = await _userAccessoryManagement.GetAccessoryListingByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// Get all accessory listings owned by current user
        [ProducesResponseType(typeof(AccessoryListingListDto), StatusCodes.Status200OK)]
        [HttpGet("my-accessory-listings")]
        public async Task<ActionResult<AccessoryListingListDto>> GetMyAccessoryListings()
        {
            var userId = GetUserId();
            var result = await _userAccessoryManagement.GetMyAccessoryListingsAsync(userId);
            return Ok(result);
        }


        /// Create a new accessory listing
        [ProducesResponseType(typeof(AccessoryOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("accessory-listing")]
        public async Task<ActionResult<AccessoryOperationResponseDto>> AddAccessoryListing([FromForm] AddAccessoryListingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userAccessoryManagement.AddAccessoryListingAsync(dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// Update an existing accessory listing
        [ProducesResponseType(typeof(AccessoryOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPut("accessory-listing/{id}")]
        public async Task<ActionResult<AccessoryOperationResponseDto>> UpdateAccessoryListing(int id, [FromForm] UpdateAccessoryListingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userAccessoryManagement.UpdateAccessoryListingAsync(id, dto, userId);
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


        /// Update the status of an accessory listing
        [ProducesResponseType(typeof(AccessoryOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPut("accessory-listing/{id}/status")]
        public async Task<ActionResult<AccessoryOperationResponseDto>> UpdateAccessoryListingStatus(int id, [FromForm] UpdateAccessoryListingStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userAccessoryManagement.UpdateAccessoryListingStatusAsync(id, userId, dto.NewStatus);
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


        /// Delete an accessory listing
        [ProducesResponseType(typeof(AccessoryOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("accessory-listing/{id}")]
        public async Task<ActionResult<AccessoryOperationResponseDto>> DeleteAccessoryListing(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userAccessoryManagement.DeleteAccessoryListingAsync(id, userId);
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



    }
}
