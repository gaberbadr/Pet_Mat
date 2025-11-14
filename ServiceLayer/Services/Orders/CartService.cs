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
using CoreLayer.Helper.Documents;
using CoreLayer.Service_Interface.Orders;
using CoreLayer.Specifications.Orders;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Orders
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<CartDto> GetCartAsync(string userId)
        {
            var spec = new CartWithItemsSpecification(userId);
            var cart = await _unitOfWork.Repository<Cart, int>().GetWithSpecficationAsync(spec);

            if (cart == null)
            {
                // Create new cart if doesn't exist
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                await _unitOfWork.Repository<Cart, int>().AddAsync(cart);
                await _unitOfWork.CompleteAsync();
            }

            return await MapCartToDto(cart);
        }

        public async Task<CartOperationResponseDto> AddToCartAsync(string userId, AddToCartDto dto)
        {
            // Validate product
            var product = await _unitOfWork.Repository<Product, int>().GetAsync(dto.ProductId);
            if (product == null || !product.IsActive)
                throw new KeyNotFoundException("Product not found or inactive");

            if (product.Stock < dto.Quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

            // Get or create cart
            var spec = new CartWithItemsSpecification(userId);
            var cart = await _unitOfWork.Repository<Cart, int>().GetWithSpecficationAsync(spec);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                await _unitOfWork.Repository<Cart, int>().AddAsync(cart);
                await _unitOfWork.CompleteAsync();
            }

            // Check if product already in cart
            var existingItem = cart.Items?.FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                // Update quantity
                var newQuantity = existingItem.Quantity + dto.Quantity;
                if (product.Stock < newQuantity)
                    throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

                existingItem.Quantity = newQuantity;
                existingItem.Price = product.Price; // Update price
                _unitOfWork.Repository<CartItem, int>().Update(existingItem);
            }
            else
            {
                // Add new item
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    Price = product.Price,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<CartItem, int>().AddAsync(cartItem);
            }

            // Recalculate discount if coupon applied
            if (!string.IsNullOrEmpty(cart.CouponCode))
            {
                await RecalculateDiscount(cart);
            }

            cart.LastUpdated = DateTime.UtcNow;
            _unitOfWork.Repository<Cart, int>().Update(cart);
            await _unitOfWork.CompleteAsync();

            var cartDto = await MapCartToDto(cart);

            return new CartOperationResponseDto
            {
                Success = true,
                Message = "Product added to cart successfully",
                Cart = cartDto
            };
        }

        public async Task<CartOperationResponseDto> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemDto dto)
        {
            var spec = new CartWithItemsSpecification(userId);
            var cart = await _unitOfWork.Repository<Cart, int>().GetWithSpecficationAsync(spec);

            if (cart == null)
                throw new KeyNotFoundException("Cart not found");

            var cartItem = cart.Items?.FirstOrDefault(i => i.Id == cartItemId);
            if (cartItem == null)
                throw new KeyNotFoundException("Cart item not found");

            // Validate stock
            var product = await _unitOfWork.Repository<Product, int>().GetAsync(cartItem.ProductId);
            if (product.Stock < dto.Quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

            cartItem.Quantity = dto.Quantity;
            cartItem.Price = product.Price; // Update to current price
            _unitOfWork.Repository<CartItem, int>().Update(cartItem);

            // Recalculate discount
            if (!string.IsNullOrEmpty(cart.CouponCode))
            {
                await RecalculateDiscount(cart);
            }

            cart.LastUpdated = DateTime.UtcNow;
            _unitOfWork.Repository<Cart, int>().Update(cart);
            await _unitOfWork.CompleteAsync();

            var cartDto = await MapCartToDto(cart);

            return new CartOperationResponseDto
            {
                Success = true,
                Message = "Cart item updated successfully",
                Cart = cartDto
            };
        }

        public async Task<CartOperationResponseDto> RemoveCartItemAsync(string userId, int cartItemId)
        {
            var spec = new CartWithItemsSpecification(userId);
            var cart = await _unitOfWork.Repository<Cart, int>().GetWithSpecficationAsync(spec);

            if (cart == null)
                throw new KeyNotFoundException("Cart not found");

            var cartItem = cart.Items?.FirstOrDefault(i => i.Id == cartItemId);
            if (cartItem == null)
                throw new KeyNotFoundException("Cart item not found");

            _unitOfWork.Repository<CartItem, int>().Delete(cartItem);

            // Recalculate discount
            if (!string.IsNullOrEmpty(cart.CouponCode))
            {
                await RecalculateDiscount(cart);
            }

            cart.LastUpdated = DateTime.UtcNow;
            _unitOfWork.Repository<Cart, int>().Update(cart);
            await _unitOfWork.CompleteAsync();

            var cartDto = await MapCartToDto(cart);

            return new CartOperationResponseDto
            {
                Success = true,
                Message = "Cart item removed successfully",
                Cart = cartDto
            };
        }

        public async Task<CartOperationResponseDto> ClearCartAsync(string userId)
        {
            var spec = new CartWithItemsSpecification(userId);
            var cart = await _unitOfWork.Repository<Cart, int>().GetWithSpecficationAsync(spec);

            if (cart == null)
                throw new KeyNotFoundException("Cart not found");

            // Delete all items
            await _unitOfWork.Repository<CartItem, int>().DeleteRangeAsync(ci => ci.CartId == cart.Id);

            // Reset cart
            cart.CouponCode = null;
            cart.DiscountAmount = 0;
            cart.LastUpdated = DateTime.UtcNow;
            _unitOfWork.Repository<Cart, int>().Update(cart);
            await _unitOfWork.CompleteAsync();

            var cartDto = await MapCartToDto(cart);

            return new CartOperationResponseDto
            {
                Success = true,
                Message = "Cart cleared successfully",
                Cart = cartDto
            };
        }

        public async Task<CartOperationResponseDto> ApplyCouponAsync(string userId, ApplyCouponDto dto)
        {
            var spec = new CartWithItemsSpecification(userId);
            var cart = await _unitOfWork.Repository<Cart, int>().GetWithSpecficationAsync(spec);

            if (cart == null)
                throw new KeyNotFoundException("Cart not found");

            if (cart.Items == null || !cart.Items.Any())
                throw new InvalidOperationException("Cart is empty");

            // Validate coupon
            var coupon = await _unitOfWork.Repository<Coupon, int>()
                .FindAsync(c => c.Code == dto.CouponCode && c.IsActive);

            var validCoupon = coupon.FirstOrDefault();
            if (validCoupon == null)
                throw new KeyNotFoundException("Invalid or inactive coupon code");

            if (validCoupon.ExpiresAt.HasValue && validCoupon.ExpiresAt.Value < DateTime.UtcNow)
                throw new InvalidOperationException("Coupon has expired");

            var subtotal = cart.Items.Sum(i => i.Price * i.Quantity);

            if (subtotal < validCoupon.MinOrderAmount)
                throw new InvalidOperationException($"Minimum order amount of {validCoupon.MinOrderAmount:C} required");

            // Calculate discount
            decimal discount = 0;
            if (validCoupon.IsPercentage)
            {
                discount = subtotal * (validCoupon.Rate / 100);
            }
            else
            {
                discount = validCoupon.Rate;
            }

            // Ensure discount doesn't exceed subtotal
            discount = Math.Min(discount, subtotal);

            cart.CouponCode = dto.CouponCode;
            cart.DiscountAmount = discount;
            cart.LastUpdated = DateTime.UtcNow;
            _unitOfWork.Repository<Cart, int>().Update(cart);
            await _unitOfWork.CompleteAsync();

            var cartDto = await MapCartToDto(cart);

            return new CartOperationResponseDto
            {
                Success = true,
                Message = "Coupon applied successfully",
                Cart = cartDto
            };
        }

        public async Task<CartOperationResponseDto> RemoveCouponAsync(string userId)
        {
            var spec = new CartWithItemsSpecification(userId);
            var cart = await _unitOfWork.Repository<Cart, int>().GetWithSpecficationAsync(spec);

            if (cart == null)
                throw new KeyNotFoundException("Cart not found");

            cart.CouponCode = null;
            cart.DiscountAmount = 0;
            cart.LastUpdated = DateTime.UtcNow;
            _unitOfWork.Repository<Cart, int>().Update(cart);
            await _unitOfWork.CompleteAsync();

            var cartDto = await MapCartToDto(cart);

            return new CartOperationResponseDto
            {
                Success = true,
                Message = "Coupon removed successfully",
                Cart = cartDto
            };
        }

        private async Task RecalculateDiscount(Cart cart)
        {
            if (string.IsNullOrEmpty(cart.CouponCode))
            {
                cart.DiscountAmount = 0;
                return;
            }

            var coupon = await _unitOfWork.Repository<Coupon, int>()
                .FindAsync(c => c.Code == cart.CouponCode && c.IsActive);

            var validCoupon = coupon.FirstOrDefault();
            if (validCoupon == null || (validCoupon.ExpiresAt.HasValue && validCoupon.ExpiresAt.Value < DateTime.UtcNow))
            {
                cart.CouponCode = null;
                cart.DiscountAmount = 0;
                return;
            }

            var subtotal = cart.Items.Sum(i => i.Price * i.Quantity);

            if (subtotal < validCoupon.MinOrderAmount)
            {
                cart.CouponCode = null;
                cart.DiscountAmount = 0;
                return;
            }

            decimal discount = 0;
            if (validCoupon.IsPercentage)
            {
                discount = subtotal * (validCoupon.Rate / 100);
            }
            else
            {
                discount = validCoupon.Rate;
            }

            cart.DiscountAmount = Math.Min(discount, subtotal);
        }

        private async Task<CartDto> MapCartToDto(Cart cart)
        {
            // Reload with items
            var spec = new CartWithItemsSpecification(cart.UserId);
            cart = await _unitOfWork.Repository<Cart, int>().GetWithSpecficationAsync(spec);

            var subtotal = cart.Items?.Sum(i => i.Price * i.Quantity) ?? 0;
            var total = subtotal - cart.DiscountAmount;

            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CouponCode = cart.CouponCode,
                DiscountAmount = cart.DiscountAmount,
                SubTotal = subtotal,
                Total = total,
                LastUpdated = cart.LastUpdated,
                Items = cart.Items?.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ProductPictureUrl = !string.IsNullOrEmpty(i.Product.PictureUrl)
                        ? DocumentSetting.GetFileUrl(i.Product.PictureUrl, "products", _configuration["BaseURL"])
                        : null,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Total = i.Price * i.Quantity,
                    Stock = i.Product.Stock,
                    BrandName = i.Product.Brand.Name
                }).ToList() ?? new List<CartItemDto>()
            };

            return cartDto;
        }
    }
 }
