using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Carts;
using CoreLayer.Entities.Identity;

namespace CoreLayer.Entities.Orders
{
    public class Order : BaseEntity<int>
    {

        [Required]
        [MaxLength(200)]
        public string BuyerEmail { get; set; }

        [Required]
        public string UserId { get; set; }

        public int? CartId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public int? DeliveryMethodId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [MaxLength(50)]
        public string CouponCode { get; set; }

        [MaxLength(200)]
        public string PaymentIntentId { get; set; }

        public string ClientSecret { get; set; }

        public int ShippingAddressId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [ForeignKey("CartId")]
        public Cart Cart { get; set; }

        [ForeignKey("DeliveryMethodId")]
        public DeliveryMethod DeliveryMethod { get; set; }

        [ForeignKey("ShippingAddressId")]
        public OrderAddress ShippingAddress { get; set; }

        public ICollection<OrderItem> Items { get; set; }
    }
}
