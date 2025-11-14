using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Orders;

namespace CoreLayer.Service_Interface.Orders
{
    public interface IDeliveryMethodService
    {
        Task<DeliveryMethodListDto> GetAllDeliveryMethodsAsync();
        Task<DeliveryMethodDto> GetDeliveryMethodByIdAsync(int id);
        Task<DeliveryMethodOperationResponseDto> AddDeliveryMethodAsync(AddDeliveryMethodDto dto);
        Task<DeliveryMethodOperationResponseDto> UpdateDeliveryMethodAsync(int id, UpdateDeliveryMethodDto dto);
        Task<DeliveryMethodOperationResponseDto> DeleteDeliveryMethodAsync(int id);
    }
}
