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
    public class PharmacyProfile : BaseEntity<int>
    {

        [Required]
        public string UserId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        [MaxLength(200)]
        public string PharmacyName { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public string Description { get; set; }

        public string WorkingHours { get; set; }

        public bool IsActive { get; set; } = true;

        public string Specializations { get; set; }

        public double AverageRating { get; set; }

        public int TotalRatings { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public ICollection<PharmacyRating> Ratings { get; set; }
        public ICollection<PharmacyListing> Listings { get; set; }
    }
}
