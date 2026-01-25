using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos;
using CoreLayer.Dtos.Accessory;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Dtos.Animals;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Accessory;
using CoreLayer.Service_Interface.User;
using CoreLayer.Specifications.Animals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;


namespace petmat.Controllers
{

    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly IUserDoctorManagement _userDoctorManagement;
        private readonly IUserAnimalManagement _userAnimalManagement;
        private readonly IUserPharmacyManagement _userPharmacyManagement;
        private readonly IUserAccessoryManagement _userAccessoryManagement;

        public UserController( IUserDoctorManagement userDoctorManagement, IUserAnimalManagement userAnimalManagement, IUserPharmacyManagement userPharmacyManagement,IUserAccessoryManagement userAccessoryManagement)
        {
            _userDoctorManagement = userDoctorManagement;
            _userAnimalManagement = userAnimalManagement;
            _userPharmacyManagement = userPharmacyManagement;
            _userAccessoryManagement = userAccessoryManagement;
        }

        private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);


        // ==================== DOCTOR APPLICATION (USER) ====================


        /// Apply to become a doctor
        [ProducesResponseType(typeof(DoctorApplicationOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [HttpPost("apply-doctor")]
        public async Task<ActionResult<DoctorApplicationOperationResponseDto>> ApplyToBeDoctor([FromForm] ApplyDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userDoctorManagement.ApplyToBeDoctorAsync(dto, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(403, new ApiErrorResponse(403, ex.Message));
            }
        }


        /// Get current user's doctor application status
        [ProducesResponseType(typeof(UserDoctorApplicationStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("doctor-application-status")]
        public async Task<ActionResult<UserDoctorApplicationStatusDto>> GetDoctorApplicationStatus()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userDoctorManagement.GetDoctorApplicationStatusAsync(userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(403, new ApiErrorResponse(403, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        // ==================== DOCTOR RATING (USER) ====================


        /// Get all doctors 
        [ProducesResponseType(typeof(PaginationResponse<PublicDoctorProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpGet("doctors")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginationResponse<PublicDoctorProfileDto>>> GetDoctors([FromQuery] DoctorFilterParams filterParams)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _userDoctorManagement.GetDoctorsAsync(filterParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Rate a doctor
        [ProducesResponseType(typeof(RatingOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("rate-doctor/{doctorId}")]
        public async Task<ActionResult<RatingOperationResponseDto>> RateDoctor(string doctorId, [FromBody] RateDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userDoctorManagement.RateDoctorAsync(doctorId, dto, userId);
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


        /// Update existing doctor rating
        [ProducesResponseType(typeof(RatingOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPut("rate-doctor/{doctorId}")]
        public async Task<ActionResult<RatingOperationResponseDto>> UpdateDoctorRating(string doctorId, [FromBody] RateDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userDoctorManagement.UpdateDoctorRatingAsync(doctorId, dto, userId);
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

 
        /// Get doctor profile by ID (public endpoint)
        [ProducesResponseType(typeof(PublicDoctorProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("doctor/{doctorId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PublicDoctorProfileDto>> GetDoctorProfile(string doctorId)
        {
            try
            {
                var result = await _userDoctorManagement.GetPublicDoctorProfileAsync(doctorId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// Get all ratings for a specific doctor
        [HttpGet("doctor/{doctorId}/all-ratings")]
        public async Task<ActionResult<DoctorRatingListDto>> GetDoctorAllRatings(string doctorId)
        {
            var ratings = await _userDoctorManagement.GetDoctorAllRatingsAsync(doctorId);
            return Ok(ratings);
        }


        // ==================== BROWSE PHARMACIES ====================


        /// Get all pharmacies with filters and pagination
        [ProducesResponseType(typeof(PaginationResponse<PublicPharmacyProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpGet("pharmacies")]
        public async Task<ActionResult<PaginationResponse<PublicPharmacyProfileDto>>> GetAllPharmacies([FromQuery] PharmacyFilterParams filterParams)
        {
            try
            {
                var result = await _userPharmacyManagement.GetPharmaciesAsync(filterParams);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        /// Get pharmacy details by user ID (pharmacy owner ID)
        [ProducesResponseType(typeof(PublicPharmacyProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("pharmacies/{pharmacyId}")]
        public async Task<ActionResult<PublicPharmacyProfileDto>> GetPharmacyById(string pharmacyId)
        {
            try
            {
                var result = await _userPharmacyManagement.GetPublicPharmacyProfileAsync(pharmacyId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        // ==================== BROWSE PHARMACY LISTINGS ====================

        /// Get all pharmacy listings (products) with filters and pagination
        [ProducesResponseType(typeof(PaginationResponse<PharmacyListingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpGet("pharmacy-listings")]
        public async Task<ActionResult<PaginationResponse<PharmacyListingResponseDto>>> GetAllPharmacyListings([FromQuery] PharmacyListingFilterParams filterParams)
        {
            try
            {
                var result = await _userPharmacyManagement.GetAllPharmacyListingsAsync(filterParams);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        /// Get pharmacy listing details by ID
        [ProducesResponseType(typeof(PharmacyListingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("pharmacy-listings/{id}")]
        public async Task<ActionResult<PharmacyListingResponseDto>> GetPharmacyListingById(int id)
        {
            try
            {
                var result = await _userPharmacyManagement.GetPharmacyListingByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// Get all listings for a specific pharmacy
        [ProducesResponseType(typeof(PaginationResponse<PharmacyListingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpGet("pharmacies/{pharmacyId}/listings")]
        public async Task<ActionResult<PaginationResponse<PharmacyListingResponseDto>>> GetListingsByPharmacyId(
            string pharmacyId, [FromQuery] PharmacyListingFilterParams filterParams)
        {
            try
            {
                var result = await _userPharmacyManagement.GetListingsByPharmacyIdAsync(pharmacyId, filterParams);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        // ==================== PHARMACY APPLICATION (USER) ====================

        /// Apply to become a pharmacy
        [ProducesResponseType(typeof(PharmacyApplicationOperationResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("apply-pharmacy")]
        public async Task<ActionResult<PharmacyApplicationOperationResponseDto>> ApplyToBePharmacy([FromForm] ApplyPharmacyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userPharmacyManagement.ApplyToBePharmacyAsync(dto, userId);
                return Created("", result);
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

        /// Get current user's pharmacy application status
        [ProducesResponseType(typeof(UserPharmacyApplicationStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("pharmacy-application-status")]
        public async Task<ActionResult<UserPharmacyApplicationStatusDto>> GetPharmacyApplicationStatus()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userPharmacyManagement.GetPharmacyApplicationStatusAsync(userId);
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

        // ==================== RATINGS (USER) ====================

        /// Rate a pharmacy
        [ProducesResponseType(typeof(RatingOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("pharmacies/{pharmacyId}/rate")]
        public async Task<ActionResult<RatingOperationResponseDto>> RatePharmacy(string pharmacyId, [FromBody] RatePharmacyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userPharmacyManagement.RatePharmacyAsync(pharmacyId, dto, userId);
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

        /// Update pharmacy rating
        [ProducesResponseType(typeof(RatingOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [HttpPut("pharmacies/{pharmacyId}/rate")]
        public async Task<ActionResult<RatingOperationResponseDto>> UpdatePharmacyRating(string pharmacyId, [FromBody] RatePharmacyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userPharmacyManagement.UpdatePharmacyRatingAsync(pharmacyId, dto, userId);
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


        /// Get all ratings for a specific pharmacy
        [HttpGet("pharmacies/{pharmacyId}/all-ratings")]
        public async Task<ActionResult<PharmacyRatingListDto>> GetPharmacyAllRatings(string pharmacyId)
        {
            var ratings = await _userPharmacyManagement.GetPharmacyAllRatingsAsync(pharmacyId);
            return Ok(ratings);
        }


    }
}

