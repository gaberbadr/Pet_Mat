using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Dtos.Orders;
using CoreLayer.Entities.Carts;
using CoreLayer.Entities.Orders;
using CoreLayer.Enums;
using CoreLayer.Service_Interface.Orders;
using CoreLayer.Specifications.Orders;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace ServiceLayer.Services.Orders
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public PaymentService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<PaymentIntentResponseDto> CreateOrUpdatePaymentIntentAsync(string userId)
        {
            StripeConfiguration.ApiKey = _configuration["Stripe:Secretkey"];

            var cartSpec = new CartWithItemsSpecification(userId);
            var cart = await _unitOfWork.Repository<Cart, int>().GetWithSpecficationAsync(cartSpec);

            if (cart == null || cart.Items == null || !cart.Items.Any())
                throw new InvalidOperationException("Cart is empty");

            // Validate and update prices
            foreach (var item in cart.Items)
            {
                var product = await _unitOfWork.Repository<CoreLayer.Entities.Foods.Product, int>().GetAsync(item.ProductId);
                if (product != null && item.Price != product.Price)
                {
                    item.Price = product.Price;
                    _unitOfWork.Repository<CartItem, int>().Update(item);
                }
            }

            var subtotal = cart.Items.Sum(i => i.Price * i.Quantity);

            // Get delivery method cost
            decimal shippingPrice = 0m;
            if (cart.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod, int>()
                    .GetAsync(cart.DeliveryMethodId.Value);
                if (deliveryMethod != null)
                {
                    shippingPrice = deliveryMethod.Cost;
                }
            }

            // Recalculate discount
            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(cart.CouponCode))
            {
                var coupons = await _unitOfWork.Repository<CoreLayer.Entities.Orders.Coupon, int>()
                    .FindAsync(c => c.Code == cart.CouponCode && c.IsActive);
                var coupon = coupons.FirstOrDefault();

                if (coupon != null &&
                    (!coupon.ExpiresAt.HasValue || coupon.ExpiresAt.Value >= DateTime.UtcNow) &&
                    subtotal >= coupon.MinOrderAmount)
                {
                    if (coupon.IsPercentage)
                    {
                        discountAmount = subtotal * (coupon.Rate / 100);
                    }
                    else
                    {
                        discountAmount = coupon.Rate;
                    }
                    discountAmount = Math.Min(discountAmount, subtotal);
                }
            }

            var total = subtotal - discountAmount + shippingPrice;
            var service = new PaymentIntentService();
            PaymentIntent paymentIntent;

            // Check if payment intent exists and its status
            if (!string.IsNullOrEmpty(cart.PaymentIntentId))
            {
                try
                {
                    var existingIntent = await service.GetAsync(cart.PaymentIntentId);

                    // If payment already succeeded, create a new payment intent
                    if (existingIntent.Status == "succeeded")
                    {
                        var options = new PaymentIntentCreateOptions
                        {
                            Amount = (long)(total * 100),
                            Currency = "usd",
                            PaymentMethodTypes = new List<string> { "card" }
                        };

                        paymentIntent = await service.CreateAsync(options);
                        cart.PaymentIntentId = paymentIntent.Id;
                        cart.ClientSecret = paymentIntent.ClientSecret;
                    }
                    // If payment can be updated
                    else if (existingIntent.Status == "requires_payment_method" ||
                             existingIntent.Status == "requires_confirmation" ||
                             existingIntent.Status == "requires_action")
                    {
                        var updateOptions = new PaymentIntentUpdateOptions
                        {
                            Amount = (long)(total * 100)
                        };

                        paymentIntent = await service.UpdateAsync(cart.PaymentIntentId, updateOptions);
                    }
                    else
                    {
                        // Create new payment intent
                        var options = new PaymentIntentCreateOptions
                        {
                            Amount = (long)(total * 100),
                            Currency = "usd",
                            PaymentMethodTypes = new List<string> { "card" }
                        };

                        paymentIntent = await service.CreateAsync(options);
                        cart.PaymentIntentId = paymentIntent.Id;
                        cart.ClientSecret = paymentIntent.ClientSecret;
                    }
                }
                catch (StripeException)
                {
                    // Create new payment intent on error
                    var options = new PaymentIntentCreateOptions
                    {
                        Amount = (long)(total * 100),
                        Currency = "usd",
                        PaymentMethodTypes = new List<string> { "card" }
                    };

                    paymentIntent = await service.CreateAsync(options);
                    cart.PaymentIntentId = paymentIntent.Id;
                    cart.ClientSecret = paymentIntent.ClientSecret;
                }
            }
            else
            {
                // Create new payment intent
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(total * 100),
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" }
                };

                paymentIntent = await service.CreateAsync(options);
                cart.PaymentIntentId = paymentIntent.Id;
                cart.ClientSecret = paymentIntent.ClientSecret;
            }

            _unitOfWork.Repository<Cart, int>().Update(cart);
            await _unitOfWork.CompleteAsync();

            return new PaymentIntentResponseDto
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = total,
                Currency = "usd"
            };
        }


        public async Task<OrderDto> UpdatePaymentIntentStatusAsync(string paymentIntentId, bool isSuccessful)
        {
            var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
            var order = await _unitOfWork.Repository<Order, int>().GetWithSpecficationAsync(spec);

            if (order == null)
                throw new KeyNotFoundException($"Order not found for payment intent: {paymentIntentId}");

            // Update status based on payment result
            order.Status = isSuccessful ? OrderStatus.Processing : OrderStatus.Cancelled;

            _unitOfWork.Repository<Order, int>().Update(order);
            await _unitOfWork.CompleteAsync();

            // Return updated order details
            var detailSpec = new AdminOrderByIdSpecification(order.Id);
            order = await _unitOfWork.Repository<Order, int>().GetWithSpecficationAsync(detailSpec);

            var deliveryCost = order.DeliveryMethod?.Cost ?? 0;
            var total = order.SubTotal - order.DiscountAmount + deliveryCost;

            return new OrderDto
            {
                Id = order.Id,
                BuyerEmail = order.BuyerEmail,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                DeliveryMethodCost = deliveryCost,
                Total = total,
                CouponCode = order.CouponCode,
                PaymentIntentId = order.PaymentIntentId
            };
        }
    }
}