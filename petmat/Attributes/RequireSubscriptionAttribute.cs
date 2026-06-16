using CoreLayer;
using CoreLayer.Entities.Doctors;
using CoreLayer.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using petmat.Errors;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace petmat.Attributes
{
    /// <summary>
    /// Place [RequireSubscription] on any doctor endpoint
    /// that requires a valid active subscription.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireSubscriptionAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var userId = context.HttpContext.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedObjectResult(
                    new ApiErrorResponse(401, "Unauthorized"));

                return;
            }

            var unitOfWork = context.HttpContext.RequestServices
                .GetRequiredService<IUnitOfWork>();

            var doctor = await unitOfWork
                .Repository<DoctorProfile, Guid>()
                .FindFirstAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                context.Result = new NotFoundObjectResult(
                    new ApiErrorResponse(404, "Doctor profile not found"));

                return;
            }

            // Subscription must be:
            // 1. Active
            // 2. Not expired
            var activeSubscription = await unitOfWork
                .Repository<DoctorSubscription, int>()
                .FindFirstAsync(s =>
                    s.DoctorId == doctor.Id &&
                    s.Status == SubscriptionStatus.Active &&
                    (
                        !s.EndDate.HasValue ||
                        s.EndDate.Value > DateTime.Now
                    ));

            if (activeSubscription == null)
            {
                context.Result = new ObjectResult(
                    new ApiErrorResponse(
                        403,
                        "You must have a valid active subscription to perform this operation."
                    ))
                {
                    StatusCode = 403
                };

                return;
            }

            await next();
        }
    }
}