using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Orders;

namespace CoreLayer.Service_Interface.Orders
{
    public interface ICouponService
    {
        Task<CouponListDto> GetAllCouponsAsync();
        Task<CouponDto> GetCouponByIdAsync(int id);
        Task<CouponDto> GetCouponByCodeAsync(string code);
        Task<CouponOperationResponseDto> AddCouponAsync(AddCouponDto dto);
        Task<CouponOperationResponseDto> UpdateCouponAsync(int id, UpdateCouponDto dto);
        Task<CouponOperationResponseDto> DeleteCouponAsync(int id);
    }
}
