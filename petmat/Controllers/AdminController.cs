using System.ComponentModel.DataAnnotations;
using CoreLayer;
using CoreLayer.Dtos;
using CoreLayer.Dtos.Admin;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ==================== USER MANAGEMENT ====================

        /// Block a user account
        [ProducesResponseType(typeof(UserBlockResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("block-user/{userId}")]
        public async Task<ActionResult<UserBlockResponseDto>> BlockUser(string userId)
        {
            try
            {
                var result = await _adminService.BlockUserAsync(userId);
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


        /// Unblock a user account
        [ProducesResponseType(typeof(UserBlockResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("unblock-user/{userId}")]
        public async Task<ActionResult<UserBlockResponseDto>> UnblockUser(string userId)
        {
            try
            {
                var result = await _adminService.UnblockUserAsync(userId);
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

        // ==================== SPECIES MANAGEMENT ====================


        /// Add a new species (or reactivate if previously deleted)
        [ProducesResponseType(typeof(SpeciesResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("species")]
        public async Task<ActionResult<SpeciesResponseDto>> AddSpecies([FromBody] SpeciesAdminDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminService.AddSpeciesAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Soft delete a species (sets IsActive to false)
        [ProducesResponseType(typeof(DeleteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("species/{id}")]
        public async Task<ActionResult<DeleteResponseDto>> DeleteSpecies(int id)
        {
            try
            {
                var result = await _adminService.DeleteSpeciesAsync(id);
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

        // ==================== SUBSPECIES MANAGEMENT ====================


        /// Add a new subspecies (or reactivate if previously deleted)
        [ProducesResponseType(typeof(SubSpeciesResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("subspecies")]
        public async Task<ActionResult<SubSpeciesResponseDto>> AddSubSpecies([FromBody] SubSpeciesAdminDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminService.AddSubSpeciesAsync(dto);
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


        /// Soft delete a subspecies (sets IsActive to false)
        [ProducesResponseType(typeof(DeleteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("subspecies/{id}")]
        public async Task<ActionResult<DeleteResponseDto>> DeleteSubSpecies(int id)
        {
            try
            {
                var result = await _adminService.DeleteSubSpeciesAsync(id);
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

        // ==================== COLOR MANAGEMENT ====================


        /// Add a new color (or reactivate if previously deleted)
        [ProducesResponseType(typeof(ColorResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("color")]
        public async Task<ActionResult<ColorResponseDto>> AddColor([FromBody] ColorAdminDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminService.AddColorAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Soft delete a color (sets IsActive to false)
        [ProducesResponseType(typeof(DeleteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("color/{id}")]
        public async Task<ActionResult<DeleteResponseDto>> DeleteColor(int id)
        {
            try
            {
                var result = await _adminService.DeleteColorAsync(id);
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


        // ==================== DOCTOR APPLICATION MANAGEMENT ====================

        /// Get all pending doctor applications
        [ProducesResponseType(typeof(DoctorApplicationListDto), StatusCodes.Status200OK)]
        [HttpGet("doctor-applications/pending")]
        public async Task<ActionResult<DoctorApplicationListDto>> GetPendingDoctorApplications()
        {
            var result = await _adminService.GetPendingDoctorApplicationsAsync();
            return Ok(result);
        }


        /// Get all doctor applications with optional status filter
        [ProducesResponseType(typeof(DoctorApplicationListDto), StatusCodes.Status200OK)]
        [HttpGet("doctor-applications")]
        public async Task<ActionResult<DoctorApplicationListDto>> GetAllDoctorApplications([FromQuery] string? status = null)
        {
            var result = await _adminService.GetAllDoctorApplicationsAsync(status);
            return Ok(result);
        }

        /// Get doctor application by ID
        [ProducesResponseType(typeof(DoctorApplicationDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("doctor-applications/{id}")]
        public async Task<ActionResult<DoctorApplicationDetailDto>> GetDoctorApplicationById(Guid id)
        {
            try
            {
                var result = await _adminService.GetDoctorApplicationByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Review doctor application (Approve or Reject)
        [ProducesResponseType(typeof(ApplicationReviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("doctor-applications/{id}/review")]
        public async Task<ActionResult<ApplicationReviewResponseDto>> ReviewDoctorApplication(
        Guid id, [FromBody] ReviewDoctorApplicationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminService.ReviewDoctorApplicationAsync(id, dto);
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
