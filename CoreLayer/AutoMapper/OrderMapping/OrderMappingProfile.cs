using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer.Dtos.Orders;
using CoreLayer.Entities.Orders;

namespace CoreLayer.AutoMapper.OrderMapping
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            CreateMap<Coupon, CouponDto>();
            CreateMap<DeliveryMethod, DeliveryMethodDto>();
        }
    }
}
