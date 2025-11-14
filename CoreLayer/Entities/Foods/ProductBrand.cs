using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Entities.Foods
{
    public class ProductBrand : BaseEntity<int>
    {

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Product> Products { get; set; }
    }
}
