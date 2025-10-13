using System;
using System.Security.Claims;
using System.Web;
using CoreLayer;
using CoreLayer.Dtos;
using CoreLayer.Dtos.Auth;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petmat.Errors;
using RepositoryLayer.Data.Context;

namespace petmat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _cfg;

        public AuthController(
            IAuthService authService,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration cfg)
        {
            _authService = authService;
            _signInManager = signInManager;
            _cfg = cfg;
        }

        // ========== Send OTP Code ==========
        [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] EmailDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponse(400, "Invalid email format"));

            var (success, message) = await _authService.SendVerificationCodeAsync(dto.Email);

            if (!success)
                return StatusCode(500, new ApiErrorResponse(500, message));

            return Ok(new SuccessResponseDto { Message = message });
        }

        // ========== Verify OTP & SignIn ==========
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponse(400, "Invalid input"));

            var (success, message, token) = await _authService.VerifyCodeAsync(dto.Email, dto.Code);

            if (!success)
            {
                return message.Contains("expired")
                    ? BadRequest(new ApiErrorResponse(400, message))
                    : Unauthorized(new ApiErrorResponse(401, message));
            }

            return Ok(token);
        }

        // ========== Email/Password Login ==========
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status429TooManyRequests)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponse(400, "Invalid input"));

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var (success, message, token, banMinutes) = await _authService.LoginAsync(dto.Email, dto.Password, ipAddress);

            if (!success)
            {
                if (banMinutes.HasValue)
                    return StatusCode(429, new ApiErrorResponse(429, message));

                return message.Contains("No password set")
                    ? BadRequest(new ApiErrorResponse(400, message))
                    : Unauthorized(new ApiErrorResponse(401, message));
            }

            return Ok(token);
        }

        // ========== Create Password ==========
        [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create-password")]
        public async Task<IActionResult> CreatePassword([FromBody] CreatePasswordDto dto)
        {
            var userId = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, message) = await _authService.CreatePasswordAsync(userId, dto.Password);

            if (!success)
                return message.Contains("not found")
                    ? Unauthorized(new ApiErrorResponse(401, message))
                    : BadRequest(new ApiErrorResponse(400, message));

            return Ok(new SuccessResponseDto { Message = message });
        }

        // ========== Update Password ==========
        [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
        {
            var userId = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, message) = await _authService.UpdatePasswordAsync(userId, dto.OldPassword, dto.NewPassword);

            if (!success)
                return message.Contains("not found")
                    ? Unauthorized(new ApiErrorResponse(401, message))
                    : BadRequest(new ApiErrorResponse(400, message));

            return Ok(new SuccessResponseDto { Message = message });
        }

        // ========== Create/Update Profile Info ==========
        [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("profile/name")]
        public async Task<IActionResult> UpdateName([FromBody] UpdateNameDto dto)
        {
            var userId = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, message) = await _authService.UpdateNameAsync(userId, dto.FirstName, dto.LastName);

            if (!success)
                return Unauthorized(new ApiErrorResponse(401, message));

            return Ok(new SuccessResponseDto { Message = message });
        }

        [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("profile/phone")]
        public async Task<IActionResult> UpdatePhone([FromBody] UpdatePhoneDto dto)
        {
            var userId = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, message) = await _authService.UpdatePhoneAsync(userId, dto.PhoneNumber);

            if (!success)
                return Unauthorized(new ApiErrorResponse(401, message));

            return Ok(new SuccessResponseDto { Message = message });
        }

        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("profile/picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] UpdateProfilePictureDto dto)
        {
            var userId = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var (success, message, pictureUrl) = await _authService.UpdateProfilePictureAsync(userId, dto.Picture, baseUrl);

            if (!success)
                return message.Contains("not found")
                    ? Unauthorized(new ApiErrorResponse(401, message))
                    : BadRequest(new ApiErrorResponse(400, message));

            return Ok(new { Message = message, ProfilePictureUrl = pictureUrl });
        }

        // ========== Address Management ==========
        [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("profile/address")]
        public async Task<IActionResult> CreateOrUpdateAddress([FromBody] AddressDto dto)
        {
            var userId = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, message) = await _authService.CreateOrUpdateAddressAsync(userId, dto);

            if (!success)
                return Unauthorized(new ApiErrorResponse(401, message));

            return Ok(new SuccessResponseDto { Message = message });
        }

        // ========== Google Login ==========
        [ProducesResponseType(StatusCodes.Status302Found)] // Redirect
        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] string returnUrl = "")
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [ProducesResponseType(StatusCodes.Status302Found)] // Redirect
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string returnUrl = "")
        {
            try
            {
                var frontendUrl = _cfg["Frontend:BaseUrl"];
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var (user, errorMessage) = await _authService.HandleGoogleCallbackAsync(ipAddress);

                if (user == null)
                {
                    var errorUrl = string.IsNullOrEmpty(returnUrl)
                        ? $"{frontendUrl}/signin.html?error={HttpUtility.UrlEncode(errorMessage)}"
                        : $"{returnUrl}?error={HttpUtility.UrlEncode(errorMessage)}";
                    return Redirect(errorUrl);
                }

                var tokenQueryString = await _authService.GenerateTokenQueryStringAsync(user, ipAddress);
                var targetUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : $"{frontendUrl}/callback.html";
                var callbackUrl = $"{targetUrl}?{tokenQueryString}";

                return Redirect(callbackUrl);
            }
            catch (Exception ex)
            {
                var frontendUrl = _cfg["Frontend:BaseUrl"];
                var errorUrl = string.IsNullOrEmpty(returnUrl)
                    ? $"{frontendUrl}/signin.html?error={HttpUtility.UrlEncode(ex.Message)}"
                    : $"{returnUrl}?error={HttpUtility.UrlEncode(ex.Message)}";
                return Redirect(errorUrl);
            }
        }

        // ========== Me ==========
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiErrorResponse(401, "User not authenticated"));

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var (success, message, profile) = await _authService.GetUserProfileAsync(userId, baseUrl);

            if (!success)
                return Unauthorized(new ApiErrorResponse(401, message));

            return Ok(profile);
        }


        // ========== another user ==========

        [ProducesResponseType(typeof(PublicUserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpGet("user/{userId}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetUserProfileById(string userId)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var (success, message, profile) = await _authService.GetPublicUserProfileAsync(userId, baseUrl);

            if (!success)
                return NotFound(new ApiErrorResponse(404, message));

            return Ok(profile);
        }

        // ========== Refresh Token ==========
        [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponse(400, "Invalid request"));

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var (success, message, token) = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

            if (!success)
                return Unauthorized(new ApiErrorResponse(401, message));

            return Ok(token);
        }

        // ========== Revoke Token ==========
        [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [HttpPost("revoke-refresh")]
        public async Task<IActionResult> Revoke([FromBody] RefreshRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResponse(400, "Invalid request"));

            var (success, message) = await _authService.RevokeTokenAsync(request.RefreshToken);

            if (!success)
                return NotFound(new ApiErrorResponse(404, message));

            return Ok(new SuccessResponseDto { Message = message });
        }

        // ========== Logout ==========
        [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiErrorResponse(401, "User not authenticated"));

            var (success, message) = await _authService.LogoutAsync(userId);

            if (!success)
                return Unauthorized(new ApiErrorResponse(401, message));

            return Ok(new SuccessResponseDto { Message = message });
        }
    }
}