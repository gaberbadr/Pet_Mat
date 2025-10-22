using System.Security.Claims;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Pharmacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    [Authorize(Roles = "Pharmacy")]
    public class PharmacyController : BaseApiController
    {
        private readonly IPharmacyService _pharmacyService;

        public PharmacyController(IPharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
        }

        private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ==================== PROFILE MANAGEMENT ====================

        /// Get current pharmacy's profile
        [ProducesResponseType(typeof(PharmacyProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("profile")]
        public async Task<ActionResult<PharmacyProfileResponseDto>> GetMyProfile()
        {
            try
            {
                var userId = GetUserId();
                var result = await _pharmacyService.GetPharmacyProfileAsync(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Update current pharmacy's profile
        [ProducesResponseType(typeof(PharmacyProfileOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPut("profile")]
        public async Task<ActionResult<PharmacyProfileOperationResponseDto>> UpdateMyProfile([FromBody] UpdatePharmacyProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _pharmacyService.UpdatePharmacyProfileAsync(userId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Update pharmacy's location (Latitude and Longitude)
        [ProducesResponseType(typeof(PharmacyProfileOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPut("location")]
        public async Task<ActionResult<PharmacyProfileOperationResponseDto>> UpdateLocation([FromBody] UpdatePharmacyLocationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _pharmacyService.UpdatePharmacyLocationAsync(userId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Delete pharmacy account (removes profile, listings, application, and pharmacy role)
        [ProducesResponseType(typeof(PharmacyProfileOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("account")]
        public async Task<ActionResult<PharmacyProfileOperationResponseDto>> DeletePharmacyAccount()
        {
            try
            {
                var userId = GetUserId();
                var result = await _pharmacyService.DeletePharmacyAccountAsync(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Get all ratings for current pharmacy
        [ProducesResponseType(typeof(PharmacyRatingListDto), StatusCodes.Status200OK)]
        [HttpGet("ratings")]
        public async Task<ActionResult<PharmacyRatingListDto>> GetMyRatings()
        {
            var userId = GetUserId();
            var result = await _pharmacyService.GetPharmacyRatingsAsync(userId);
            return Ok(result);
        }

        // ==================== PHARMACY LISTINGS (PRODUCTS) ====================


        /// Get all listings (products) for current pharmacy with filters and pagination
        [ProducesResponseType(typeof(PaginationResponse<PharmacyListingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpGet("listings")]
        public async Task<ActionResult<PaginationResponse<PharmacyListingResponseDto>>> GetMyListings([FromQuery] PharmacyListingFilterParams filterParams)
        {
            try
            {
                var userId = GetUserId();
                var result = await _pharmacyService.GetMyListingsAsync(userId, filterParams);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Get a specific listing by ID
        [ProducesResponseType(typeof(PharmacyListingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [HttpGet("listings/{id}")]
        public async Task<ActionResult<PharmacyListingResponseDto>> GetMyListingById(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _pharmacyService.GetMyListingByIdAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponse(401, ex.Message));
            }
        }


        /// Add a new product listing
        [ProducesResponseType(typeof(PharmacyListingOperationResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("listings")]
        public async Task<ActionResult<PharmacyListingOperationResponseDto>> AddListing([FromForm] AddPharmacyListingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _pharmacyService.AddListingAsync(dto, userId);
                return CreatedAtAction(nameof(GetMyListingById), new { id = result.ListingId }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Update an existing listing
        [ProducesResponseType(typeof(PharmacyListingOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [HttpPut("listings/{id}")]
        public async Task<ActionResult<PharmacyListingOperationResponseDto>> UpdateListing(int id, [FromForm] UpdatePharmacyListingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _pharmacyService.UpdateListingAsync(id, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponse(401, ex.Message));
            }
        }


        /// Delete a listing (soft delete)
        [ProducesResponseType(typeof(PharmacyListingOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [HttpDelete("listings/{id}")]
        public async Task<ActionResult<PharmacyListingOperationResponseDto>> DeleteListing(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _pharmacyService.DeleteListingAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponse(401, ex.Message));
            }
        }


        /// Update listing stock quantity
        [ProducesResponseType(typeof(PharmacyListingOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [HttpPatch("listings/{id}/stock")]
        public async Task<ActionResult<PharmacyListingOperationResponseDto>> UpdateListingStock(int id, [FromBody] UpdateListingStockDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _pharmacyService.UpdateListingStockAsync(id, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponse(401, ex.Message));
            }
        }
    }
}
