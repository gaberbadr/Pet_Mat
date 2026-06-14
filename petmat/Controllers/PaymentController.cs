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
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, IConfiguration configuration, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _logger = logger;
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

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent == null)
                    {
                        _logger.LogError("PaymentIntent is null for payment_intent.succeeded event");
                        return BadRequest(new ApiErrorResponse(400, "Invalid PaymentIntent data"));
                    }

                    try
                    {
                        await _paymentService.UpdatePaymentIntentStatusAsync(paymentIntent.Id, true);
                        _logger.LogInformation("Order status updated successfully for {PaymentIntentId}", paymentIntent.Id);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        _logger.LogWarning("Order not found for PaymentIntentId: {PaymentIntentId}", paymentIntent.Id);
                        return Ok();
                    }
                }
                else if (stripeEvent.Type == "payment_intent.payment_failed")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent == null)
                    {
                        _logger.LogError("PaymentIntent is null for payment_intent.payment_failed event");
                        return BadRequest(new ApiErrorResponse(400, "Invalid PaymentIntent data"));
                    }

                    try
                    {
                        await _paymentService.UpdatePaymentIntentStatusAsync(paymentIntent.Id, false);
                        _logger.LogInformation("Order cancelled successfully for {PaymentIntentId}", paymentIntent.Id);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        _logger.LogWarning("Order not found for PaymentIntentId: {PaymentIntentId}", paymentIntent.Id);
                        return Ok();
                    }
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError("StripeException: {Message}", ex.Message);
                return BadRequest(new ApiErrorResponse(400, "Webhook signature verification failed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Stripe webhook");
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }
    }
}
