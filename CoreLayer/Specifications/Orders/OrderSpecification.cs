using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Carts;
using CoreLayer.Entities.Orders;
using CoreLayer.Enums;

namespace CoreLayer.Specifications.Orders
{
    // Cart Specifications
    public class CartWithItemsSpecification : BaseSpecifications<Cart, int>
    {
        public CartWithItemsSpecification(string userId)
            : base(c => c.UserId == userId)
        {
            Includes.Add(c => c.Items);
            Includes.Add(c => c.Items.Select(i => i.Product).FirstOrDefault());
            Includes.Add(c => c.Items.Select(i => i.Product.Brand).FirstOrDefault());
        }
    }

    // Order Specifications
    public class OrderWithDetailsSpecification : BaseSpecifications<Order, int>
    {
        public OrderWithDetailsSpecification(int orderId, string userId)
            : base(o => o.Id == orderId && o.UserId == userId)
        {
            Includes.Add(o => o.Items);
            Includes.Add(o => o.Items.Select(i => i.Product).FirstOrDefault());
            Includes.Add(o => o.DeliveryMethod);
            Includes.Add(o => o.ShippingAddress);
        }
    }

    public class OrdersByUserSpecification : BaseSpecifications<Order, int>
    {
        public OrdersByUserSpecification(string userId)
            : base(o => o.UserId == userId)
        {
            Includes.Add(o => o.Items);
            Includes.Add(o => o.Items.Select(i => i.Product).FirstOrDefault());
            Includes.Add(o => o.DeliveryMethod);
            Includes.Add(o => o.ShippingAddress);
            OrderByDescending = o => o.OrderDate;
        }
    }

    public class OrderByPaymentIntentIdSpecification : BaseSpecifications<Order, int>
    {
        public OrderByPaymentIntentIdSpecification(string paymentIntentId)
            : base(o => o.PaymentIntentId == paymentIntentId)
        {
            Includes.Add(o => o.Items);
            Includes.Add(o => o.Items.Select(i => i.Product).FirstOrDefault());
            Includes.Add(o => o.DeliveryMethod);
            Includes.Add(o => o.ShippingAddress);
        }
    }

    // Admin Order Specifications
    public class AdminOrdersSpecification : BaseSpecifications<Order, int>
    {
        public AdminOrdersSpecification(int pageIndex, int pageSize, OrderStatus? status = null)
            : base(o => !status.HasValue || o.Status == status.Value)
        {
            Includes.Add(o => o.Items);
            Includes.Add(o => o.Items.Select(i => i.Product).FirstOrDefault());
            Includes.Add(o => o.DeliveryMethod);
            Includes.Add(o => o.ShippingAddress);
            Includes.Add(o => o.User);

            OrderByDescending = o => o.OrderDate;

            applyPagnation((pageIndex - 1) * pageSize, pageSize);
        }
    }

    public class AdminOrdersCountSpecification : BaseSpecifications<Order, int>
    {
        public AdminOrdersCountSpecification(OrderStatus? status = null)
            : base(o => !status.HasValue || o.Status == status.Value)
        {
        }
    }

    public class AdminOrderByIdSpecification : BaseSpecifications<Order, int>
    {
        public AdminOrderByIdSpecification(int orderId)
            : base(o => o.Id == orderId)
        {
            Includes.Add(o => o.Items);
            Includes.Add(o => o.Items.Select(i => i.Product).FirstOrDefault());
            Includes.Add(o => o.DeliveryMethod);
            Includes.Add(o => o.ShippingAddress);
            Includes.Add(o => o.User);
        }
    }
}
