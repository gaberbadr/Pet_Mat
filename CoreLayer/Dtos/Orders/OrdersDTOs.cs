using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Enums;

namespace CoreLayer.Dtos.Orders
{
    // ==================== CART DTOs ====================

    public class AddToCartDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    public class ApplyCouponDto
    {
        [Required]
        [MaxLength(50)]
        public string CouponCode { get; set; }
    }

    public class CartDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public DateTime LastUpdated { get; set; }
    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductPictureUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public int Stock { get; set; }
        public string BrandName { get; set; }
    }

    // ==================== ORDER DTOs ====================

    public class CreateOrderDto
    {
        [Required]
        public int DeliveryMethodId { get; set; }

        [Required]
        public OrderAddressDto ShippingAddress { get; set; }
    }

    public class OrderAddressDto
    {
        [Required]
        [MaxLength(100)]
        public string FName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LName { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(200)]
        public string Street { get; set; }

        [Required]
        [MaxLength(100)]
        public string Country { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string BuyerEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DeliveryMethodCost { get; set; }
        public decimal Total { get; set; }
        public string CouponCode { get; set; }
        public string PaymentIntentId { get; set; }
        public OrderAddressDto ShippingAddress { get; set; }
        public DeliveryMethodDto DeliveryMethod { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductPictureUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }

    public class DeliveryMethodDto
    {
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string DeliveryTime { get; set; }
        public decimal Cost { get; set; }
    }

    public class OrderListDto
    {
        public int Count { get; set; }
        public IEnumerable<OrderDto> Data { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }

    // ==================== COUPON DTOs ====================

    public class CouponDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Rate { get; set; }
        public bool IsPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public decimal MinOrderAmount { get; set; }
    }

    public class AddCouponDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Rate { get; set; }

        [Required]
        public bool IsPercentage { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinOrderAmount { get; set; }
    }

    public class UpdateCouponDto
    {
        [MaxLength(200)]
        public string Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Rate { get; set; }

        public bool? IsPercentage { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinOrderAmount { get; set; }
    }

    public class CouponListDto
    {
        public int Count { get; set; }
        public IEnumerable<CouponDto> Data { get; set; }
    }

    // ==================== DELIVERY METHOD DTOs ====================

    public class AddDeliveryMethodDto
    {
        [Required]
        [MaxLength(100)]
        public string ShortName { get; set; }

        public string Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string DeliveryTime { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }
    }

    public class UpdateDeliveryMethodDto
    {
        [MaxLength(100)]
        public string ShortName { get; set; }

        public string Description { get; set; }

        [MaxLength(100)]
        public string DeliveryTime { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Cost { get; set; }
    }

    public class DeliveryMethodListDto
    {
        public int Count { get; set; }
        public IEnumerable<DeliveryMethodDto> Data { get; set; }
    }

    // ==================== RESPONSE DTOs ====================

    public class CartOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CartDto Cart { get; set; }
    }

    public class OrderOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? OrderId { get; set; }
    }

    public class CouponOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? CouponId { get; set; }
    }

    public class DeliveryMethodOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? DeliveryMethodId { get; set; }
    }

    // ==================== PAYMENT DTOs ====================

    public class PaymentIntentResponseDto
    {
        public string PaymentIntentId { get; set; }
        public string ClientSecret { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
