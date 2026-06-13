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
                Console.WriteLine($"[Stripe Webhook] Event Data Object Type: {stripeEvent.Data.Object?.GetType().Name}");

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent == null)
                    {
                        Console.WriteLine("[Stripe Webhook] ERROR: PaymentIntent is null for payment_intent.succeeded event");
                        return BadRequest(new ApiErrorResponse(400, "Invalid PaymentIntent data"));
                    }

                    Console.WriteLine($"[Stripe Webhook] Processing payment_intent.succeeded");
                    Console.WriteLine($"[Stripe Webhook] PaymentIntent ID: {paymentIntent.Id}");
                    Console.WriteLine($"[Stripe Webhook] Amount: {paymentIntent.Amount}");
                    Console.WriteLine($"[Stripe Webhook] Status: {paymentIntent.Status}");

                    try
                    {
                        await _paymentService.UpdatePaymentIntentStatusAsync(paymentIntent.Id, true);
                        Console.WriteLine($"[Stripe Webhook] ✓ Order status updated successfully for {paymentIntent.Id}");
                    }
                    catch (KeyNotFoundException ex)
                    {
                        Console.WriteLine($"[Stripe Webhook] ⚠ Order not found for PaymentIntentId: {paymentIntent.Id}");
                        Console.WriteLine($"[Stripe Webhook] Exception: {ex.Message}");
                        // Still return 200 to acknowledge receipt (Stripe will retry if we return error)
                        // This prevents duplicate webhooks
                        return Ok();
                    }
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

                    try
                    {
                        await _paymentService.UpdatePaymentIntentStatusAsync(paymentIntent.Id, false);
                        Console.WriteLine($"[Stripe Webhook] ✓ Order cancelled successfully for {paymentIntent.Id}");
                    }
                    catch (KeyNotFoundException ex)
                    {
                        Console.WriteLine($"[Stripe Webhook] ⚠ Order not found for PaymentIntentId: {paymentIntent.Id}");
                        Console.WriteLine($"[Stripe Webhook] Exception: {ex.Message}");
                        return Ok();
                    }
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
            catch (Exception ex)
            {
                Console.WriteLine($"[Stripe Webhook] Unexpected error: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"[Stripe Webhook] Stack Trace: {ex.StackTrace}");
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }
    }
}
