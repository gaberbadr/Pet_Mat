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
            // Cart -> Items -> Product -> Brand
            AddInclude("Items.Product.Brand");
        }
    }

    // Order Specifications
    public class OrderWithDetailsSpecification : BaseSpecifications<Order, int>
    {
        public OrderWithDetailsSpecification(int orderId, string userId)
            : base(o => o.Id == orderId && o.UserId == userId)
        {
            // Load nested Order -> Items -> Product
            AddInclude("Items.Product");

            // Load top-level properties
            Includes.Add(o => o.DeliveryMethod);
            Includes.Add(o => o.ShippingAddress);
        }
    }

    public class OrdersByUserSpecification : BaseSpecifications<Order, int>
    {
        public OrdersByUserSpecification(string userId)
            : base(o => o.UserId == userId)
        {
            // Load nested Order -> Items -> Product
            AddInclude("Items.Product");

            // Load top-level properties
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
            // Load nested Order -> Items -> Product
            AddInclude("Items.Product");

            // Load top-level properties
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
            // Load nested Order -> Items -> Product
            AddInclude("Items.Product");

            // Load top-level properties
            Includes.Add(o => o.DeliveryMethod);
            Includes.Add(o => o.ShippingAddress);
            Includes.Add(o => o.User);

            OrderByDescending = o => o.OrderDate;
            applyPagnation((pageIndex - 1) * pageSize, pageSize);
        }
    }

    public class AdminOrdersCountSpecification : BaseSpecifications<Order, int>
    {
        // No includes needed, so no changes
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
            // Load nested Order -> Items -> Product
            AddInclude("Items.Product");

            // Load top-level properties
            Includes.Add(o => o.DeliveryMethod);
            Includes.Add(o => o.ShippingAddress);
            Includes.Add(o => o.User);
        }
    }

    public class ExpiredPendingPaymentOrdersSpecification : BaseSpecifications<Order, int>
    {
        public ExpiredPendingPaymentOrdersSpecification(DateTime cutoff)
            : base(o => o.Status == OrderStatus.PendingPayment && o.OrderDate <= cutoff)
        {
            // include order items and product for stock adjustments
            AddInclude("Items.Product");
        }
    }
}