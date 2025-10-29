using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
using CoreLayer.Enums;

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
        public AccessoryCondition Condition { get; set; }

        [MaxLength(100)]
        public AccessoryCategory Category { get; set; }

        [MaxLength(50)]
        public ListingStatus Status { get; set; } = ListingStatus.Active;

        public string ImageUrls { get; set; }

        [Required]
        public string OwnerId { get; set; }

        public int? SpeciesId { get; set; }

        public bool IsActive { get; set; } = true;


        [MaxLength(200)]

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; }

        [ForeignKey("SpeciesId")]
        public Species Species { get; set; }
    }
}
