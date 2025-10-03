using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Carts;
using CoreLayer.Entities.Orders;

namespace CoreLayer.Entities.Foods
{
    public class Product : BaseEntity<int>
    {

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [MaxLength(500)]
        public string PictureUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        public int BrandId { get; set; }

        public int TypeId { get; set; }

        public int? SpeciesId { get; set; }

        public string NutritionalInfo { get; set; }

        public string Ingredients { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("BrandId")]
        public ProductBrand Brand { get; set; }

        [ForeignKey("TypeId")]
        public ProductType Type { get; set; }

        [ForeignKey("SpeciesId")]
        public Species Species { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
