using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Entities.Orders
{
    public class DeliveryMethod : BaseEntity<int>
    {

        [Required]
        [MaxLength(100)]
        public string ShortName { get; set; }

        public string Description { get; set; }

        [MaxLength(100)]
        public string DeliveryTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Order> Orders { get; set; }
    }
}
