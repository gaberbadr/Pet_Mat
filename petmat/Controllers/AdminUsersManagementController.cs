using CoreLayer.Dtos.Admin;
using CoreLayer.Service_Interface.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;
using static CoreLayer.Dtos.Admin.AdminUsersManagementDTOs;

namespace petmat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersManagementController : BaseApiController
    {
        private readonly IAdminUserManagement _adminUserManagement;

        public AdminUsersManagementController(IAdminUserManagement adminUserManagement)
        {
            _adminUserManagement = adminUserManagement;
        }

        // ==================== USER BLOCKING ====================


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

        // ==================== ROLE MANAGEMENT ====================


        /// Add AdminAssistant role to a user
        [ProducesResponseType(typeof(RoleOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("add-admin-assistant/{userId}")]
        public async Task<ActionResult<RoleOperationResponseDto>> AddAdminAssistant(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminUserManagement.AddAdminAssistantRoleAsync(userId);
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

        /// Remove Doctor role and all related data from a user
        [ProducesResponseType(typeof(RoleOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("remove-doctor-role/{userId}")]
        public async Task<ActionResult<RoleOperationResponseDto>> RemoveDoctorRole(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminUserManagement.RemoveDoctorRoleAsync(userId);
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


        /// Remove Pharmacy role and all related data from a user
        [ProducesResponseType(typeof(RoleOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("remove-pharmacy-role/{userId}")]
        public async Task<ActionResult<RoleOperationResponseDto>> RemovePharmacyRole(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminUserManagement.RemovePharmacyRoleAsync(userId);
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


        /// Remove AdminAssistant role from a user
        [ProducesResponseType(typeof(RoleOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("remove-admin-assistant/{userId}")]
        public async Task<ActionResult<RoleOperationResponseDto>> RemoveAdminAssistant(string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var result = await _adminUserManagement.RemoveAdminAssistantRoleAsync(userId);
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
