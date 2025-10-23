using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Dtos.User;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.User;
using CoreLayer.Specifications.User;
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

        public UserController( IUserDoctorManagement userDoctorManagement, IUserAnimalManagement userAnimalManagement, IUserPharmacyManagement userPharmacyManagement)
        {
            _userDoctorManagement = userDoctorManagement;
            _userAnimalManagement = userAnimalManagement;
            _userPharmacyManagement = userPharmacyManagement;
        }

        private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ==================== ANIMAL MANAGEMENT ====================


        /// Get all animals owned by current user
        [ProducesResponseType(typeof(AnimalListDto), StatusCodes.Status200OK)]
        [HttpGet("my-animals")]
        public async Task<ActionResult<AnimalListDto>> GetMyAnimals()
        {
            var userId = GetUserId();
            var result = await _userAnimalManagement.GetMyAnimalsAsync(userId);
            return Ok(result);
        }


        /// Add a new animal
        [ProducesResponseType(typeof(AnimalOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("animal")]
        public async Task<ActionResult<AnimalOperationResponseDto>> AddAnimal([FromForm] AddAnimalDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userAnimalManagement.AddAnimalAsync(dto, userId);
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


        /// Update an existing animal
        [ProducesResponseType(typeof(AnimalOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPut("animal/{id}")]
        public async Task<ActionResult<AnimalOperationResponseDto>> UpdateAnimal(int id, [FromForm] UpdateAnimalDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userAnimalManagement.UpdateAnimalAsync(id, dto, userId);
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Delete an animal
        [ProducesResponseType(typeof(AnimalOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("animal/{id}")]
        public async Task<ActionResult<AnimalOperationResponseDto>> DeleteAnimal(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userAnimalManagement.DeleteAnimalAsync(id, userId);
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

        // ==================== ANIMAL LISTINGS ====================


        /// Get all animal listings with filters and pagination
        [ProducesResponseType(typeof(PaginationResponse<AnimalListingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpGet("listings")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginationResponse<AnimalListingResponseDto>>> GetAllListings([FromQuery] AnimalListingFilterParams filterParams)
        {
            try
            {
                var result = await _userAnimalManagement.GetAllListingsAsync(filterParams);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, "An error occurred while fetching listings"));
            }
        }

        /// Get animal listing by ID
        [ProducesResponseType(typeof(AnimalListingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("listing/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AnimalListingResponseDto>> GetListingById(int id)
        {
            try
            {
                var result = await _userAnimalManagement.GetListingByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// Get all animal listings owned by current user
        [ProducesResponseType(typeof(AnimalListingListDto), StatusCodes.Status200OK)]
        [HttpGet("my-listings")]
        public async Task<ActionResult<AnimalListingListDto>> GetMyListings()
        {
            var userId = GetUserId();
            var result = await _userAnimalManagement.GetMyListingsAsync(userId);
            return Ok(result);
        }


        /// Create a new animal listing
        [ProducesResponseType(typeof(ListingOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("listing")]
        public async Task<ActionResult<ListingOperationResponseDto>> AddAnimalListing([FromBody] AddAnimalListingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userAnimalManagement.AddAnimalListingAsync(dto, userId);
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

        /// Update the status of an animal listing
        [ProducesResponseType(typeof(ListingOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPut("listing/{id}/status")]
        public async Task<ActionResult<ListingOperationResponseDto>> UpdateListingStatus(int id, [FromBody] UpdateListingStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _userAnimalManagement.UpdateListingStatusAsync(id, userId, dto.NewStatus);
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
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Delete an animal listing
        [ProducesResponseType(typeof(ListingOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("listing/{id}")]
        public async Task<ActionResult<ListingOperationResponseDto>> DeleteAnimalListing(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userAnimalManagement.DeleteAnimalListingAsync(id, userId);
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

        // ==================== SPECIES INFO ====================


        /// Get all species
        [ProducesResponseType(typeof(SpeciesListDto), StatusCodes.Status200OK)]
        [HttpGet("species")]
        [AllowAnonymous]
        public async Task<ActionResult<SpeciesListDto>> GetAllSpecies()
        {
            var result = await _userAnimalManagement.GetAllSpeciesAsync();
            return Ok(result);
        }

        /// Get all subspecies
        [ProducesResponseType(typeof(SubSpeciesListDto), StatusCodes.Status200OK)]
        [HttpGet("subspecies")]
        [AllowAnonymous]
        public async Task<ActionResult<SubSpeciesListDto>> GetAllSubSpecies()
        {
            var result = await _userAnimalManagement.GetAllSubSpeciesAsync();
            return Ok(result);
        }

        /// Get subspecies by species ID
        [ProducesResponseType(typeof(SubSpeciesListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("subspecies/species/{speciesId}")]
        [AllowAnonymous]
        public async Task<ActionResult<SubSpeciesListDto>> GetSubSpeciesBySpeciesId(int speciesId)
        {
            try
            {
                var result = await _userAnimalManagement.GetSubSpeciesBySpeciesIdAsync(speciesId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        /// Get all colors
        [ProducesResponseType(typeof(ColorListDto), StatusCodes.Status200OK)]
        [HttpGet("colors")]
        [AllowAnonymous]
        public async Task<ActionResult<ColorListDto>> GetAllColors()
        {
            var result = await _userAnimalManagement.GetAllColorsAsync();
            return Ok(result);
        }

        // Add these endpoints to the existing UserController class

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

    }
}

