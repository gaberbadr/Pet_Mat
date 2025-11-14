using System.ComponentModel.DataAnnotations;
using CoreLayer;
using CoreLayer.Dtos;
using CoreLayer.Dtos.Admin;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
using CoreLayer.Enums;
using CoreLayer.Service_Interface.Admin;
using CoreLayer.Specifications.Animals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseApiController
    {

        private readonly IAdminAnimalManagement _adminAnimalManagement;
        private readonly IAdminUserManagement _adminUserManagement;
        private readonly IAdminDoctorApplicationManagement _adminDoctorApplicationManagement;
        private readonly IAdminPharmacyApplicationManagement _adminPharmacyApplicationManagement;

        public AdminController(IAdminUserManagement adminUserManagement,
            IAdminAnimalManagement adminAnimalManagement
            ,IAdminDoctorApplicationManagement adminDoctorApplicationManagement,
            IAdminPharmacyApplicationManagement adminPharmacyApplicationManagement)
        {
            _adminAnimalManagement = adminAnimalManagement;
            _adminUserManagement = adminUserManagement;
            _adminDoctorApplicationManagement = adminDoctorApplicationManagement;
            _adminPharmacyApplicationManagement = adminPharmacyApplicationManagement;
        }

        // ==================== USER MANAGEMENT ====================

        /// Block a user account
        [ProducesResponseType(typeof(UserBlockResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("block-user/{userId}")]
        public async Task<ActionResult<UserBlockResponseDto>> BlockUser(string userId)
        {

            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminUserManagement.BlockUserAsync(userId);
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
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("unblock-user/{userId}")]
        public async Task<ActionResult<UserBlockResponseDto>> UnblockUser(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminUserManagement.UnblockUserAsync(userId);
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
                var result = await _adminAnimalManagement.AddSpeciesAsync(dto);
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
                var result = await _adminAnimalManagement.DeleteSpeciesAsync(id);
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
                var result = await _adminAnimalManagement.AddSubSpeciesAsync(dto);
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
                var result = await _adminAnimalManagement.DeleteSubSpeciesAsync(id);
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
                var result = await _adminAnimalManagement.AddColorAsync(dto);
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
                var result = await _adminAnimalManagement.DeleteColorAsync(id);
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
            var result = await _adminDoctorApplicationManagement.GetPendingDoctorApplicationsAsync();
            return Ok(result);
        }

        //get all doctor application or get all by status
        [ProducesResponseType(typeof(DoctorApplicationListDto), StatusCodes.Status200OK)]
        [HttpGet("doctor-applications")]
        public async Task<ActionResult<DoctorApplicationListDto>> GetAllDoctorApplications([FromQuery] ApplicationStatus? status = null)
        {
            var result = await _adminDoctorApplicationManagement.GetAllDoctorApplicationsAsync(status);
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
                var result = await _adminDoctorApplicationManagement.GetDoctorApplicationByIdAsync(id);
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
        Guid id, [FromForm] ReviewDoctorApplicationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminDoctorApplicationManagement.ReviewDoctorApplicationAsync(id, dto);
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


        // ==================== PHARMACY APPLICATION MANAGEMENT ====================


        /// Get all pending pharmacy applications
        [ProducesResponseType(typeof(PharmacyApplicationListDto), StatusCodes.Status200OK)]
        [HttpGet("pharmacy-applications/pending")]
        public async Task<ActionResult<PharmacyApplicationListDto>> GetPendingPharmacyApplications()
        {
            var result = await _adminPharmacyApplicationManagement.GetPendingPharmacyApplicationsAsync();
            return Ok(result);
        }


        /// Get all pharmacy applications with optional status filter
        [ProducesResponseType(typeof(PharmacyApplicationListDto), StatusCodes.Status200OK)]
        [HttpGet("pharmacy-applications")]
        public async Task<ActionResult<PharmacyApplicationListDto>> GetAllPharmacyApplications([FromQuery] ApplicationStatus? status = null)
        {
            var result = await _adminPharmacyApplicationManagement.GetAllPharmacyApplicationsAsync(status);
            return Ok(result);
        }


        /// Get pharmacy application by ID
        [ProducesResponseType(typeof(PharmacyApplicationDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("pharmacy-applications/{id}")]
        public async Task<ActionResult<PharmacyApplicationDetailDto>> GetPharmacyApplicationById(Guid id)
        {
            try
            {
                var result = await _adminPharmacyApplicationManagement.GetPharmacyApplicationByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Review pharmacy application (Approve or Reject)
        [ProducesResponseType(typeof(ApplicationReviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("pharmacy-applications/{id}/review")]
        public async Task<ActionResult<ApplicationReviewResponseDto>> ReviewPharmacyApplication(
            Guid id, [FromForm] ReviewPharmacyApplicationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminPharmacyApplicationManagement.ReviewPharmacyApplicationAsync(id, dto);
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
