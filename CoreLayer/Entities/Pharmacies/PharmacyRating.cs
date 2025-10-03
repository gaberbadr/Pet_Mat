using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;

namespace CoreLayer.Entities.Pharmacies
{
    public class PharmacyRating : BaseEntity<int>
    {

        [Required]
        public string PharmacyId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Review { get; set; }

        [Range(1, 5)]
        public int ServiceRating { get; set; }

        [Range(1, 5)]
        public int ProductAvailabilityRating { get; set; }

        [Range(1, 5)]
        public int PricingRating { get; set; }

        [Range(1, 5)]
        public int LocationRating { get; set; }

        public bool IsVerifiedExperience { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("PharmacyId")]
        public ApplicationUser Pharmacy { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
