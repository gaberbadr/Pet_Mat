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
    public class DeliveryMethodService : IDeliveryMethodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeliveryMethodService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DeliveryMethodListDto> GetAllDeliveryMethodsAsync()
        {
            var methods = await _unitOfWork.Repository<DeliveryMethod, int>().GetAllAsync();
            var methodDtos = _mapper.Map<IEnumerable<DeliveryMethodDto>>(methods);

            return new DeliveryMethodListDto
            {
                Count = methodDtos.Count(),
                Data = methodDtos
            };
        }

        public async Task<DeliveryMethodDto> GetDeliveryMethodByIdAsync(int id)
        {
            var method = await _unitOfWork.Repository<DeliveryMethod, int>().GetAsync(id);
            if (method == null)
                throw new KeyNotFoundException("Delivery method not found");

            return _mapper.Map<DeliveryMethodDto>(method);
        }

        public async Task<DeliveryMethodOperationResponseDto> AddDeliveryMethodAsync(AddDeliveryMethodDto dto)
        {
            var method = new DeliveryMethod
            {
                ShortName = dto.ShortName,
                Description = dto.Description,
                DeliveryTime = dto.DeliveryTime,
                Cost = dto.Cost,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<DeliveryMethod, int>().AddAsync(method);
            await _unitOfWork.CompleteAsync();

            return new DeliveryMethodOperationResponseDto
            {
                Success = true,
                Message = "Delivery method created successfully",
                DeliveryMethodId = method.Id
            };
        }

        public async Task<DeliveryMethodOperationResponseDto> UpdateDeliveryMethodAsync(int id, UpdateDeliveryMethodDto dto)
        {
            var method = await _unitOfWork.Repository<DeliveryMethod, int>().GetAsync(id);
            if (method == null)
                throw new KeyNotFoundException("Delivery method not found");

            if (!string.IsNullOrEmpty(dto.ShortName)) method.ShortName = dto.ShortName;
            if (!string.IsNullOrEmpty(dto.Description)) method.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.DeliveryTime)) method.DeliveryTime = dto.DeliveryTime;
            if (dto.Cost.HasValue) method.Cost = dto.Cost.Value;

            _unitOfWork.Repository<DeliveryMethod, int>().Update(method);
            await _unitOfWork.CompleteAsync();

            return new DeliveryMethodOperationResponseDto
            {
                Success = true,
                Message = "Delivery method updated successfully",
                DeliveryMethodId = id
            };
        }

        public async Task<DeliveryMethodOperationResponseDto> DeleteDeliveryMethodAsync(int id)
        {
            var method = await _unitOfWork.Repository<DeliveryMethod, int>().GetAsync(id);
            if (method == null)
                throw new KeyNotFoundException("Delivery method not found");

            // Check if used in any orders
            var orders = await _unitOfWork.Repository<Order, int>()
                .FindAsync(o => o.DeliveryMethodId == id);

            if (orders.Any())
                throw new InvalidOperationException($"Cannot delete delivery method. It's used in {orders.Count()} order(s)");

            _unitOfWork.Repository<DeliveryMethod, int>().Delete(method);
            await _unitOfWork.CompleteAsync();

            return new DeliveryMethodOperationResponseDto
            {
                Success = true,
                Message = "Delivery method deleted successfully",
                DeliveryMethodId = id
            };
        }
    }

}
