using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Dtos.Orders;
using CoreLayer.Entities.Carts;
using CoreLayer.Entities.Orders;
using CoreLayer.Entities.Foods;
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

            // Create a fresh payment intent for the current cart totals.
            var options = new PaymentIntentCreateOptions
            {
                // Round to nearest cent to avoid floating-point truncation
                Amount = (long)Math.Round(total * 100m),
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "cartId", cart.Id.ToString() }
                }
            };

            PaymentIntent paymentIntent;
            try
            {
                paymentIntent = await service.CreateAsync(options);
            }
            catch (StripeException ex)
            {
                // Surface a friendly message to the caller; controllers map InvalidOperationException to BadRequest
                throw new InvalidOperationException($"Payment gateway error: {ex.Message}");
            }

            // Persist any cart item price updates done above
            await _unitOfWork.CompleteAsync();

            return new PaymentIntentResponseDto
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = (decimal)paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency,
                Status = paymentIntent.Status
            };
        }

        public async Task<OrderDto> UpdatePaymentIntentStatusAsync(string paymentIntentId, bool isSuccessful)
        {
            var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
            var order = await _unitOfWork.Repository<Order, int>().GetWithSpecficationAsync(spec);

            if (order == null)
                throw new KeyNotFoundException($"Order not found for payment intent: {paymentIntentId}");

            // If success -> move to Processing (idempotent)
            if (isSuccessful)
            {
                if (order.Status != OrderStatus.Processing)
                {
                    order.Status = OrderStatus.Processing;
                    _unitOfWork.Repository<Order, int>().Update(order);
                    await _unitOfWork.CompleteAsync();
                }
            }
            else
            {
                // On payment failure -> cancel order and restore stock (idempotent)
                if (order.Status != OrderStatus.Cancelled)
                {
                    // Restore product stock
                    foreach (var item in order.Items)
                    {
                        var product = await _unitOfWork.Repository<CoreLayer.Entities.Foods.Product, int>().GetAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Stock += item.Quantity;
                            _unitOfWork.Repository<CoreLayer.Entities.Foods.Product, int>().Update(product);
                        }
                    }

                    order.Status = OrderStatus.Cancelled;
                    _unitOfWork.Repository<Order, int>().Update(order);
                    await _unitOfWork.CompleteAsync();
                }
            }

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