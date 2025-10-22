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
    public class DoctorApply : BaseEntity<Guid>
    {

        [Required]
        public string UserId { get; set; }

        [MaxLength(200)]
        public string Specialization { get; set; }

        public int ExperienceYears { get; set; }

        [MaxLength(500)]
        public string ClinicAddress { get; set; }

        [MaxLength(500)]
        public string NationalIdFront { get; set; }

        [MaxLength(500)]
        public string NationalIdBack { get; set; }

        [MaxLength(500)]
        public string SelfieWithId { get; set; }

        [MaxLength(500)]
        public string SyndicateCard { get; set; }

        [MaxLength(500)]
        public string MedicalLicense { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public string? RejectionReason { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
