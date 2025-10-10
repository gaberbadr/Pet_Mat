using System.Security.Claims;
using CoreLayer.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using petmat.Errors;

namespace petmat.Middleware
{
    public class UserActiveStatusMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserActiveStatusMiddleware> _logger;

        public UserActiveStatusMiddleware(RequestDelegate next, ILogger<UserActiveStatusMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            // Skip middleware for non-authenticated requests
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Get user ID from claims
            var userId = context.User.FindFirstValue("uid") ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                await _next(context);
                return;
            }

            // Check if user exists and is active
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ApiErrorResponse(401, "User not found"));
                return;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Blocked user {UserId} attempted to access the system", userId);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ApiErrorResponse(403, "Your account has been blocked. Please contact support."));
                return;
            }

            await _next(context);
        }
    }
}
