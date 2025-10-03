using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Orders;

namespace CoreLayer.Entities.Carts
{
    public class Cart : BaseEntity<int>
    {
        [Required]
        public string UserId { get; set; }

        [MaxLength(50)]
        public string CouponCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public ICollection<CartItem> Items { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
