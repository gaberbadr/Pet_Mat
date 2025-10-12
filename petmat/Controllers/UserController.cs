using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos;
using CoreLayer.Dtos.User;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface;
using CoreLayer.Specifications.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;


namespace petmat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        // ==================== ANIMAL MANAGEMENT ====================


        /// Get all animals owned by current user
        [ProducesResponseType(typeof(AnimalListDto), StatusCodes.Status200OK)]
        [HttpGet("my-animals")]
        public async Task<ActionResult<AnimalListDto>> GetMyAnimals()
        {
            var userId = GetUserId();
            var result = await _userService.GetMyAnimalsAsync(userId);
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
                var result = await _userService.AddAnimalAsync(dto, userId);
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
                var result = await _userService.UpdateAnimalAsync(id, dto, userId);
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
                var result = await _userService.DeleteAnimalAsync(id, userId);
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
                var result = await _userService.GetAllListingsAsync(filterParams);
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
                var result = await _userService.GetListingByIdAsync(id);
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
            var result = await _userService.GetMyListingsAsync(userId);
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
                var result = await _userService.AddAnimalListingAsync(dto, userId);
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
                var result = await _userService.DeleteAnimalListingAsync(id, userId);
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
            var result = await _userService.GetAllSpeciesAsync();
            return Ok(result);
        }

        /// Get all subspecies
        [ProducesResponseType(typeof(SubSpeciesListDto), StatusCodes.Status200OK)]
        [HttpGet("subspecies")]
        [AllowAnonymous]
        public async Task<ActionResult<SubSpeciesListDto>> GetAllSubSpecies()
        {
            var result = await _userService.GetAllSubSpeciesAsync();
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
                var result = await _userService.GetSubSpeciesBySpeciesIdAsync(speciesId);
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
            var result = await _userService.GetAllColorsAsync();
            return Ok(result);
        }
    }
}

