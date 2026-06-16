using CoreLayer.Service_Interface.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;
using Stripe;

namespace petmat.Controllers
{
    /// <summary>
    /// Stripe webhook receiver for doctor subscription payments.
    /// Route: api/doctor/subscription/webhook
    /// Register this URL in your Stripe dashboard under "doctor subscription" events.
    /// </summary>
    public class DoctorPaymentWebhookController : BaseApiController
    {
        private readonly IDoctorSubscriptionService _subscriptionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DoctorPaymentWebhookController> _logger;

        public DoctorPaymentWebhookController(
            IDoctorSubscriptionService subscriptionService,
            IConfiguration configuration,
            ILogger<DoctorPaymentWebhookController> logger)
        {
            _subscriptionService = subscriptionService;
            _configuration = configuration;
            _logger = logger;
        }

        // POST api/doctor/subscription/webhook
        [HttpPost("webhook")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = _configuration["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                // ── payment succeeded ────────────────────────────────────────
                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    // Only handle subscription intents (check metadata)
                    if (paymentIntent?.Metadata?.ContainsKey("type") == true &&
                        paymentIntent.Metadata["type"] == "doctor_subscription")
                    {
                        try
                        {
                            await _subscriptionService.UpdateSubscriptionStatusAsync(paymentIntent.Id, true);
                            _logger.LogInformation(
                                "Doctor subscription activated for PaymentIntent {Id}", paymentIntent.Id);
                        }
                        catch (KeyNotFoundException)
                        {
                            _logger.LogWarning(
                                "No subscription found for PaymentIntent {Id} (succeeded)", paymentIntent.Id);
                        }
                    }
                }

                // ── payment failed ───────────────────────────────────────────
                else if (stripeEvent.Type == "payment_intent.payment_failed")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    if (paymentIntent?.Metadata?.ContainsKey("type") == true &&
                        paymentIntent.Metadata["type"] == "doctor_subscription")
                    {
                        try
                        {
                            await _subscriptionService.UpdateSubscriptionStatusAsync(paymentIntent.Id, false);
                            _logger.LogInformation(
                                "Doctor subscription payment failed for PaymentIntent {Id}", paymentIntent.Id);
                        }
                        catch (KeyNotFoundException)
                        {
                            _logger.LogWarning(
                                "No subscription found for PaymentIntent {Id} (failed)", paymentIntent.Id);
                        }
                    }
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError("StripeException in doctor webhook: {Message}", ex.Message);
                return BadRequest(new ApiErrorResponse(400, "Webhook signature verification failed"));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in doctor subscription webhook");
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }
    }
}
