using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Orders;
using CoreLayer.Entities.Orders;
using CoreLayer.Service_Interface.Orders;

namespace ServiceLayer.Services.Orders
{
    public class CouponService : ICouponService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CouponService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CouponListDto> GetAllCouponsAsync()
        {
            var coupons = await _unitOfWork.Repository<Coupon, int>().GetAllAsync();
            var couponDtos = _mapper.Map<IEnumerable<CouponDto>>(coupons);

            return new CouponListDto
            {
                Count = couponDtos.Count(),
                Data = couponDtos
            };
        }

        public async Task<CouponDto> GetCouponByIdAsync(int id)
        {
            var coupon = await _unitOfWork.Repository<Coupon, int>().GetAsync(id);
            if (coupon == null)
                throw new KeyNotFoundException("Coupon not found");

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<CouponDto> GetCouponByCodeAsync(string code)
        {
            var coupons = await _unitOfWork.Repository<Coupon, int>()
                .FindAsync(c => c.Code == code);

            var coupon = coupons.FirstOrDefault();
            if (coupon == null)
                throw new KeyNotFoundException("Coupon not found");

            return _mapper.Map<CouponDto>(coupon);
        }

        public async Task<CouponOperationResponseDto> AddCouponAsync(AddCouponDto dto)
        {
            // Check if code already exists
            var existing = await _unitOfWork.Repository<Coupon, int>()
                .FindAsync(c => c.Code == dto.Code);

            if (existing.Any())
                throw new InvalidOperationException("Coupon code already exists");

            var coupon = new Coupon
            {
                Name = dto.Name,
                Code = dto.Code,
                Rate = dto.Rate,
                IsPercentage = dto.IsPercentage,
                IsActive = true,
                ExpiresAt = dto.ExpiresAt,
                MinOrderAmount = dto.MinOrderAmount,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Coupon, int>().AddAsync(coupon);
            await _unitOfWork.CompleteAsync();

            return new CouponOperationResponseDto
            {
                Success = true,
                Message = "Coupon created successfully",
                CouponId = coupon.Id
            };
        }

        public async Task<CouponOperationResponseDto> UpdateCouponAsync(int id, UpdateCouponDto dto)
        {
            var coupon = await _unitOfWork.Repository<Coupon, int>().GetAsync(id);
            if (coupon == null)
                throw new KeyNotFoundException("Coupon not found");

            if (!string.IsNullOrEmpty(dto.Name)) coupon.Name = dto.Name;
            if (dto.Rate.HasValue) coupon.Rate = dto.Rate.Value;
            if (dto.IsPercentage.HasValue) coupon.IsPercentage = dto.IsPercentage.Value;
            if (dto.IsActive.HasValue) coupon.IsActive = dto.IsActive.Value;
            if (dto.ExpiresAt.HasValue) coupon.ExpiresAt = dto.ExpiresAt;
            if (dto.MinOrderAmount.HasValue) coupon.MinOrderAmount = dto.MinOrderAmount.Value;

            _unitOfWork.Repository<Coupon, int>().Update(coupon);
            await _unitOfWork.CompleteAsync();

            return new CouponOperationResponseDto
            {
                Success = true,
                Message = "Coupon updated successfully",
                CouponId = id
            };
        }

        public async Task<CouponOperationResponseDto> DeleteCouponAsync(int id)
        {
            var coupon = await _unitOfWork.Repository<Coupon, int>().GetAsync(id);
            if (coupon == null)
                throw new KeyNotFoundException("Coupon not found");

            _unitOfWork.Repository<Coupon, int>().Delete(coupon);
            await _unitOfWork.CompleteAsync();

            return new CouponOperationResponseDto
            {
                Success = true,
                Message = "Coupon deleted successfully",
                CouponId = id
            };
        }
    }
}


