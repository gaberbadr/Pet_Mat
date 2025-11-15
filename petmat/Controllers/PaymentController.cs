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

                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    await _paymentService.UpdatePaymentIntentStatusAsync(paymentIntent.Id, true);
                }
                else if (stripeEvent.Type == "payment_intent.payment_failed")
                {
                    await _paymentService.UpdatePaymentIntentStatusAsync(paymentIntent.Id, false);
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest(new ApiErrorResponse(400, "Webhook signature verification failed"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }
    }
}
