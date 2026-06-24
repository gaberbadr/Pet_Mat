using CoreLayer.Dtos.Doctor;
using CoreLayer.Service_Interface.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;
using System.Security.Claims;

namespace petmat.Controllers
{
    /// <summary>
    /// Doctor subscription endpoints.
    /// Base route: api/doctor/subscription
    /// </summary>
    [Authorize(Roles = "Doctor")]
    public class DoctorSubscriptionController : BaseApiController
    {
        private readonly IDoctorSubscriptionService _subscriptionService;
        private readonly IPackageService _packageService;
        private readonly IDoctorService _doctorService;

        public DoctorSubscriptionController(
            IDoctorSubscriptionService subscriptionService,
            IPackageService packageService,
            IDoctorService doctorService)
        {
            _subscriptionService = subscriptionService;
            _packageService = packageService;
            _doctorService = doctorService;
        }

        // ── Public (no subscription required) ────────────────────────────────

        /// <summary>List all active packages a doctor can choose from.</summary>
        // GET api/doctor/subscription/packages
        [HttpGet("packages")]
        [ProducesResponseType(typeof(IEnumerable<PackageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PackageDto>>> GetAvailablePackages()
        {
            var packages = await _packageService.GetAllPackagesAsync(includeInactive: false);
            return Ok(packages);
        }

        /// <summary>Get current active subscription (if any).</summary>
        // GET api/doctor/subscription/active
        [HttpGet("active")]
        [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveSubscription()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Resolve doctor profile Id by userId
            var profile = await _doctorService.GetDoctorProfileAsync(userId);
            var doctorProfileId = profile.Id.ToString();

            var sub
                = await _subscriptionService.GetActiveSubscriptionAsync(doctorProfileId);
            return sub == null ? Ok("you don't choose subscription yet") : Ok(sub);
        }

        // ── Payment initiation ────────────────────────────────────────────────

        /// <summary>
        /// Doctor selects a package → server creates a Stripe PaymentIntent and returns the ClientSecret.
        /// The frontend uses the ClientSecret to complete payment via Stripe.js / Stripe SDK.
        /// After payment the webhook activates the subscription automatically.
        /// </summary>
        // POST api/doctor/subscription/pay
        [HttpPost("pay")]
        [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SubscriptionDto>> InitiateSubscriptionPayment([FromBody] CreateSubscriptionDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                // Resolve doctor profile Id from the authenticated user's id
                var profile = await _doctorService.GetDoctorProfileAsync(userId);
                var doctorProfileId = profile.Id.ToString();

                var result = await _subscriptionService.CreateSubscriptionPaymentAsync(doctorProfileId, dto);
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

        /// <summary>Delete the current active subscription.</summary>
        // DELETE api/doctor/subscription/cancel
        [HttpDelete("cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelSubscription()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                // Resolve doctor profile Id from the authenticated user's id
                var profile = await _doctorService.GetDoctorProfileAsync(userId);
                var doctorProfileId = profile.Id.ToString();

                await _subscriptionService.DeleteSubscriptionAsync(doctorProfileId);
                return Ok(new { message = "Subscription cancelled successfully" });
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
