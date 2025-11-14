using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Orders;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Orders;
using CoreLayer.Enums;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Orders;
using CoreLayer.Specifications.Orders;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Orders
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AdminOrderService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<PaginationResponse<OrderDto>> GetAllOrdersAsync(int pageIndex, int pageSize, string status = null)
        {
            if (pageIndex < 1)
                throw new ArgumentException("PageIndex must be greater than 0");

            if (pageSize < 1)
                throw new ArgumentException("PageSize must be greater than 0");

            OrderStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
                {
                    statusEnum = parsedStatus;
                }
            }

            var spec = new AdminOrdersSpecification(pageIndex, pageSize, statusEnum);
            var countSpec = new AdminOrdersCountSpecification(statusEnum);

            var orders = await _unitOfWork.Repository<Order, int>().GetAllWithSpecficationAsync(spec);
            var totalCount = await _unitOfWork.Repository<Order, int>().GetCountAsync(countSpec);

            var orderDtos = orders.Select(o => MapOrderToDto(o)).ToList();

            return new PaginationResponse<OrderDto>(pageSize, pageIndex, totalCount, orderDtos);
        }

        public async Task<OrderDto> GetOrderByIdAsync(int orderId)
        {
            var spec = new AdminOrderByIdSpecification(orderId);
            var order = await _unitOfWork.Repository<Order, int>().GetWithSpecficationAsync(spec);

            if (order == null)
                throw new KeyNotFoundException("Order not found");

            return MapOrderToDto(order);
        }

        public async Task<OrderOperationResponseDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto)
        {
            var order = await _unitOfWork.Repository<Order, int>().GetAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found");

            if (order.Status == dto.Status)
            {
                return new OrderOperationResponseDto
                {
                    Success = false,
                    Message = $"Order status is already '{dto.Status}'",
                    OrderId = orderId
                };
            }

            order.Status = dto.Status;
            _unitOfWork.Repository<Order, int>().Update(order);
            await _unitOfWork.CompleteAsync();

            return new OrderOperationResponseDto
            {
                Success = true,
                Message = $"Order status updated to '{dto.Status}' successfully",
                OrderId = orderId
            };
        }

        public async Task<OrderOperationResponseDto> DeleteOrderAsync(int orderId)
        {
            var spec = new AdminOrderByIdSpecification(orderId);
            var order = await _unitOfWork.Repository<Order, int>().GetWithSpecficationAsync(spec);

            if (order == null)
                throw new KeyNotFoundException("Order not found");

            // Restore product stock
            foreach (var item in order.Items)
            {
                var product = await _unitOfWork.Repository<Product, int>().GetAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    _unitOfWork.Repository<Product, int>().Update(product);
                }
            }

            // Delete order (cascade will delete items and address)
            _unitOfWork.Repository<Order, int>().Delete(order);
            await _unitOfWork.CompleteAsync();

            return new OrderOperationResponseDto
            {
                Success = true,
                Message = "Order deleted successfully and stock restored",
                OrderId = orderId
            };
        }

        private OrderDto MapOrderToDto(Order order)
        {
            var deliveryCost = order.DeliveryMethod?.Cost ?? 0;
            var total = order.SubTotal - order.DiscountAmount + deliveryCost;

            return new OrderDto
            {
                Id = order.Id,
                BuyerEmail = order.BuyerEmail,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                DeliveryMethodCost = deliveryCost,
                Total = total,
                CouponCode = order.CouponCode,
                PaymentIntentId = order.PaymentIntentId,
                ShippingAddress = new OrderAddressDto
                {
                    FName = order.ShippingAddress.FName,
                    LName = order.ShippingAddress.LName,
                    City = order.ShippingAddress.City,
                    Street = order.ShippingAddress.Street,
                    Country = order.ShippingAddress.Country
                },
                DeliveryMethod = order.DeliveryMethod != null ? new DeliveryMethodDto
                {
                    Id = order.DeliveryMethod.Id,
                    ShortName = order.DeliveryMethod.ShortName,
                    Description = order.DeliveryMethod.Description,
                    DeliveryTime = order.DeliveryMethod.DeliveryTime,
                    Cost = order.DeliveryMethod.Cost
                } : null,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductName = i.Product.Name,
                    ProductPictureUrl = !string.IsNullOrEmpty(i.Product.PictureUrl)
                        ? DocumentSetting.GetFileUrl(i.Product.PictureUrl, "products", _configuration["BaseURL"])
                        : null,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    Total = i.Price * i.Quantity
                }).ToList()
            };
        }
    }
}
