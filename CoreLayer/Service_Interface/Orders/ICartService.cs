using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Orders;

namespace CoreLayer.Service_Interface.Orders
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string userId);
        Task<CartOperationResponseDto> AddToCartAsync(string userId, AddToCartDto dto);
        Task<CartOperationResponseDto> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemDto dto);
        Task<CartOperationResponseDto> RemoveCartItemAsync(string userId, int cartItemId);
        Task<CartOperationResponseDto> ClearCartAsync(string userId);
        Task<CartOperationResponseDto> ApplyCouponAsync(string userId, ApplyCouponDto dto);
        Task<CartOperationResponseDto> RemoveCouponAsync(string userId);
    }
}
