using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Orders;

namespace CoreLayer.Service_Interface.Orders
{
    public interface IPaymentService
    {
        Task<PaymentIntentResponseDto> CreateOrUpdatePaymentIntentAsync(string userId);
        Task<OrderDto> UpdatePaymentIntentStatusAsync(string paymentIntentId, bool isSuccessful);
    }
}
