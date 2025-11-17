using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Orders;

namespace CoreLayer.Service_Interface.Orders
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(string userId, string buyerEmail, CreateOrderDto dto);
        Task<OrderDto> GetOrderByIdAsync(string userId, int orderId);
        Task<OrderListDto> GetUserOrdersAsync(string userId);
        Task<PaymentIntentResponseDto> CreateOrUpdatePaymentIntentAsync(string userId);
        Task<bool> ValidateOrderExistsForPaymentAsync(string paymentIntentId);
    }
}
