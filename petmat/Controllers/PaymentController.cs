using CoreLayer.Service_Interface.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;
using Stripe;

namespace petmat.Controllers
{

    public class PaymentController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }


        /// Stripe webhook endpoint for payment status updates
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

                Console.WriteLine($"[Stripe Webhook] Event Type: {stripeEvent.Type}");
                Console.WriteLine($"[Stripe Webhook] Event ID: {stripeEvent.Id}");

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent == null)
                    {
                        Console.WriteLine("[Stripe Webhook] ERROR: PaymentIntent is null for payment_intent.succeeded event");
                        return BadRequest(new ApiErrorResponse(400, "Invalid PaymentIntent data"));
                    }

                    Console.WriteLine($"[Stripe Webhook] Processing payment_intent.succeeded: {paymentIntent.Id}");
                    await _paymentService.UpdatePaymentIntentStatusAsync(paymentIntent.Id, true);
                }
                else if (stripeEvent.Type == "payment_intent.payment_failed")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent == null)
                    {
                        Console.WriteLine("[Stripe Webhook] ERROR: PaymentIntent is null for payment_intent.payment_failed event");
                        return BadRequest(new ApiErrorResponse(400, "Invalid PaymentIntent data"));
                    }

                    Console.WriteLine($"[Stripe Webhook] Processing payment_intent.payment_failed: {paymentIntent.Id}");
                    await _paymentService.UpdatePaymentIntentStatusAsync(paymentIntent.Id, false);
                }
                else
                {
                    Console.WriteLine($"[Stripe Webhook] Ignoring event type: {stripeEvent.Type}");
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"[Stripe Webhook] StripeException: {ex.Message}");
                return BadRequest(new ApiErrorResponse(400, "Webhook signature verification failed"));
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[Stripe Webhook] KeyNotFoundException: {ex.Message}");
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Stripe Webhook] Unexpected error: {ex.GetType().Name} - {ex.Message}");
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }
    }
}
