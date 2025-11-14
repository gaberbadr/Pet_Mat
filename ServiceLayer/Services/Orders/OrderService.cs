using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Orders;
using CoreLayer.Entities.Carts;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Orders;
using CoreLayer.Enums;
using CoreLayer.Helper.Documents;
using CoreLayer.Service_Interface.Orders;
using CoreLayer.Specifications.Orders;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPaymentService paymentService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _paymentService = paymentService;
            _configuration = configuration;
        }

        public async Task<OrderDto> CreateOrderAsync(string userId, string buyerEmail, CreateOrderDto dto)
        {
            // Get cart with items
            var cartSpec = new CartWithItemsSpecification(userId);
            var cart = await _unitOfWork.Repository<Cart, int>().GetWithSpecficationAsync(cartSpec);

            if (cart == null || cart.Items == null || !cart.Items.Any())
                throw new InvalidOperationException("Cart is empty");

            // Validate delivery method
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod, int>().GetAsync(dto.DeliveryMethodId);
            if (deliveryMethod == null)
                throw new KeyNotFoundException("Delivery method not found");

            // Validate and create order items
            var orderItems = new List<OrderItem>();
            foreach (var cartItem in cart.Items)
            {
                var product = await _unitOfWork.Repository<Product, int>().GetAsync(cartItem.ProductId);

                if (product == null || !product.IsActive)
                    throw new InvalidOperationException($"Product {cartItem.ProductId} is not available");

                if (product.Stock < cartItem.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for {product.Name}. Available: {product.Stock}");

                // Use current product price (not cart price)
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Price = product.Price,
                    Quantity = cartItem.Quantity,
                    CreatedAt = DateTime.UtcNow
                };
                orderItems.Add(orderItem);

                // Reduce stock
                product.Stock -= cartItem.Quantity;
                _unitOfWork.Repository<Product, int>().Update(product);
            }

            // Calculate totals
            var subtotal = orderItems.Sum(i => i.Price * i.Quantity);

            // Validate coupon again
            decimal discountAmount = 0;
            string couponCode = null;
            if (!string.IsNullOrEmpty(cart.CouponCode))
            {
                var coupon = await _unitOfWork.Repository<Coupon, int>()
                    .FindAsync(c => c.Code == cart.CouponCode && c.IsActive);
                var validCoupon = coupon.FirstOrDefault();

                if (validCoupon != null &&
                    (!validCoupon.ExpiresAt.HasValue || validCoupon.ExpiresAt.Value >= DateTime.UtcNow) &&
                    subtotal >= validCoupon.MinOrderAmount)
                {
                    if (validCoupon.IsPercentage)
                    {
                        discountAmount = subtotal * (validCoupon.Rate / 100);
                    }
                    else
                    {
                        discountAmount = validCoupon.Rate;
                    }
                    discountAmount = Math.Min(discountAmount, subtotal);
                    couponCode = cart.CouponCode;
                }
            }

            // Create shipping address
            var shippingAddress = new OrderAddress
            {
                FName = dto.ShippingAddress.FName,
                LName = dto.ShippingAddress.LName,
                City = dto.ShippingAddress.City,
                Street = dto.ShippingAddress.Street,
                Country = dto.ShippingAddress.Country
            };
            await _unitOfWork.Repository<OrderAddress, int>().AddAsync(shippingAddress);
            await _unitOfWork.CompleteAsync();

            // Check for existing order with same payment intent
            if (!string.IsNullOrEmpty(cart.PaymentIntentId))
            {
                var existingOrderSpec = new OrderByPaymentIntentIdSpecification(cart.PaymentIntentId);
                var existingOrder = await _unitOfWork.Repository<Order, int>()
                    .GetWithSpecficationAsync(existingOrderSpec);

                if (existingOrder != null)
                {
                    _unitOfWork.Repository<Order, int>().Delete(existingOrder);
                    await _unitOfWork.CompleteAsync();
                }
            }

            // Create payment intent
            var paymentIntent = await _paymentService.CreateOrUpdatePaymentIntentAsync(userId);

            // Create order
            var order = new Order
            {
                BuyerEmail = buyerEmail,
                UserId = userId,
                CartId = cart.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                DeliveryMethodId = dto.DeliveryMethodId,
                SubTotal = subtotal,
                DiscountAmount = discountAmount,
                CouponCode = couponCode,
                PaymentIntentId = paymentIntent.PaymentIntentId,
                ClientSecret = paymentIntent.ClientSecret,
                ShippingAddressId = shippingAddress.Id,
                Items = orderItems,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Order, int>().AddAsync(order);
            await _unitOfWork.CompleteAsync();

            // Clear cart after successful order
            await _unitOfWork.Repository<CartItem, int>().DeleteRangeAsync(ci => ci.CartId == cart.Id);
            cart.CouponCode = null;
            cart.DiscountAmount = 0;
            cart.PaymentIntentId = null;
            cart.ClientSecret = null;
            cart.LastUpdated = DateTime.UtcNow;
            _unitOfWork.Repository<Cart, int>().Update(cart);
            await _unitOfWork.CompleteAsync();

            return await GetOrderByIdAsync(userId, order.Id);
        }

        public async Task<OrderDto> GetOrderByIdAsync(string userId, int orderId)
        {
            var spec = new OrderWithDetailsSpecification(orderId, userId);
            var order = await _unitOfWork.Repository<Order, int>().GetWithSpecficationAsync(spec);

            if (order == null)
                throw new KeyNotFoundException("Order not found");

            return MapOrderToDto(order);
        }

        public async Task<OrderListDto> GetUserOrdersAsync(string userId)
        {
            var spec = new OrdersByUserSpecification(userId);
            var orders = await _unitOfWork.Repository<Order, int>().GetAllWithSpecficationAsync(spec);

            var orderDtos = orders.Select(o => MapOrderToDto(o)).ToList();

            return new OrderListDto
            {
                Count = orderDtos.Count,
                Data = orderDtos
            };
        }

        public async Task<PaymentIntentResponseDto> CreateOrUpdatePaymentIntentAsync(string userId)
        {
            return await _paymentService.CreateOrUpdatePaymentIntentAsync(userId);
        }

        private OrderDto MapOrderToDto(Order order)
        {
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
                PaymentIntentId = order.PaymentIntentId,
                ShippingAddress = new OrderAddressDto
                {
                    FName = order.ShippingAddress.FName,
                    LName = order.ShippingAddress.LName,
                    City = order.ShippingAddress.City,
                    Street = order.ShippingAddress.Street,
                    Country = order.ShippingAddress.Country
                },
                DeliveryMethod = order.DeliveryMethod != null ? new DeliveryMethodDto
                {
                    Id = order.DeliveryMethod.Id,
                    ShortName = order.DeliveryMethod.ShortName,
                    Description = order.DeliveryMethod.Description,
                    DeliveryTime = order.DeliveryMethod.DeliveryTime,
                    Cost = order.DeliveryMethod.Cost
                } : null,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductName = i.Product.Name,
                    ProductPictureUrl = !string.IsNullOrEmpty(i.Product.PictureUrl)
                        ? DocumentSetting.GetFileUrl(i.Product.PictureUrl, "products", _configuration["BaseURL"])
                        : null,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Total = i.Price * i.Quantity
                }).ToList()
            };
        }
    }
}
