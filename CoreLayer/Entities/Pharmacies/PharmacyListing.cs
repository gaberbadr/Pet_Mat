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

namespace CoreLayer.Entities.Pharmacies
{
    public class PharmacyListing : BaseEntity<int>
    {

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        public string ImageUrls { get; set; }

        [Required]
        public string PharmacyId { get; set; }

        public int? SpeciesId { get; set; }

        public PharmacyListingCategory Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("PharmacyId")]
        public ApplicationUser Pharmacy { get; set; }

        [ForeignKey("SpeciesId")]
        public Species Species { get; set; }
    }
}
