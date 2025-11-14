using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Orders;
using CoreLayer.Helper.Pagination;

namespace CoreLayer.Service_Interface.Orders
{
    public interface IAdminOrderService
    {
        Task<PaginationResponse<OrderDto>> GetAllOrdersAsync(int pageIndex, int pageSize, string status = null);
        Task<OrderDto> GetOrderByIdAsync(int orderId);
        Task<OrderOperationResponseDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto);
        Task<OrderOperationResponseDto> DeleteOrderAsync(int orderId);
    }
}
