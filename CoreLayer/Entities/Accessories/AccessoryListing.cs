using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;

namespace CoreLayer.Entities.Accessories
{
    public class AccessoryListing : BaseEntity<int>
    {

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string Condition { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public string ImageUrls { get; set; }

        [Required]
        public string OwnerId { get; set; }

        public int? SpeciesId { get; set; }

        public double Latitude { get; set; }

        public bool IsActive { get; set; } = true;

        public double Longitude { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; }

        [ForeignKey("SpeciesId")]
        public Species Species { get; set; }
    }
}
