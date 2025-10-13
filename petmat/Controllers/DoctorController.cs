using System.Security.Claims;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Service_Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);


        /// Get current doctor's profile
        [ProducesResponseType(typeof(DoctorProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("profile")]
        public async Task<ActionResult<DoctorProfileResponseDto>> GetMyProfile()
        {
            try
            {
                var userId = GetUserId();
                var result = await _doctorService.GetDoctorProfileAsync(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Update current doctor's profile
        [ProducesResponseType(typeof(DoctorProfileOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPut("profile")]
        public async Task<ActionResult<DoctorProfileOperationResponseDto>> UpdateMyProfile([FromBody] UpdateDoctorProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _doctorService.UpdateDoctorProfileAsync(userId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Update doctor's location (Latitude and Longitude)
        [ProducesResponseType(typeof(DoctorProfileOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPatch("location")]
        public async Task<ActionResult<DoctorProfileOperationResponseDto>> UpdateLocation([FromBody] UpdateDoctorLocationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiValidationErrorResponse());

            try
            {
                var userId = GetUserId();
                var result = await _doctorService.UpdateDoctorLocationAsync(userId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Delete doctor account (removes profile, application, and doctor role)
        [ProducesResponseType(typeof(DoctorProfileOperationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("account")]
        public async Task<ActionResult<DoctorProfileOperationResponseDto>> DeleteDoctorAccount()
        {
            try
            {
                var userId = GetUserId();
                var result = await _doctorService.DeleteDoctorAccountAsync(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Get all ratings for current doctor
        [ProducesResponseType(typeof(DoctorRatingListDto), StatusCodes.Status200OK)]
        [HttpGet("ratings")]
        public async Task<ActionResult<DoctorRatingListDto>> GetMyRatings()
        {
            var userId = GetUserId();
            var result = await _doctorService.GetDoctorRatingsAsync(userId);
            return Ok(result);
        }
    }
}
