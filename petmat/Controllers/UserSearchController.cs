using System.Security.Claims;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petmat.Errors;

namespace petmat.Controllers
{
    [Authorize]
    public class UserSearchController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserSearchController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        private string GetUserId() =>
     User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("search")]
        [ProducesResponseType(typeof(PaginationResponse<UserSearchResultDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResponse<UserSearchResultDto>>> SearchUsers(
            [FromQuery] string query = "",
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageIndex < 1 || pageSize < 1)
                return BadRequest(new ApiErrorResponse(400, "PageIndex and PageSize must be greater than 0"));

            var baseUrl = _configuration["BaseURL"];
            var currentUserId = GetUserId();

            // Only active users AND not the current user
            var usersQuery = _userManager.Users
                .Where(u => u.IsActive && u.Id != currentUserId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();

                usersQuery = usersQuery.Where(u =>
                    (u.FirstName != null && EF.Functions.Like(u.FirstName.ToLower(), $"%{q}%")) ||
                    (u.LastName != null && EF.Functions.Like(u.LastName.ToLower(), $"%{q}%")) ||
                    (u.Email != null && EF.Functions.Like(u.Email.ToLower(), $"%{q}%"))
                );
            }

            // Pagination
            var totalCount = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var results = users.Select(u => new UserSearchResultDto
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email,
                ProfilePicture = !string.IsNullOrEmpty(u.ProfilePicture)
                    ? DocumentSetting.GetFileUrl(u.ProfilePicture, "profiles", baseUrl)
                    : null
            }).ToList();

            return Ok(new PaginationResponse<UserSearchResultDto>(
                pageSize, pageIndex, totalCount, results));
        }
    }

        // ==================== DTOs ====================

        public class UserSearchResultDto
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
    }
}
