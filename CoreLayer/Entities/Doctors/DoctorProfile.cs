using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;

namespace CoreLayer.Entities.Doctors
{
    public class DoctorProfile : BaseEntity<Guid>
    {

        [Required]
        public string UserId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        [MaxLength(200)]
        public string Specialization { get; set; }

        public int ExperienceYears { get; set; }

        [MaxLength(500)]
        public string ClinicAddress { get; set; }

        [MaxLength(200)]
        public string? ClinicName { get; set; }

        public string? Bio { get; set; }

        public string? WorkingHours { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public bool IsAvailableForConsultation { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public string? Services { get; set; }

        public string? Languages { get; set; }

        public double AverageRating { get; set; }

        public int TotalRatings { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public ICollection<DoctorRating> Ratings { get; set; }
    }

}
