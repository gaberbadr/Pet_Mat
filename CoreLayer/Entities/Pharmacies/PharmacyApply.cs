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
    public class PharmacyApply : BaseEntity<int>
    {

        [Required]
        public string UserId { get; set; }

        [MaxLength(200)]
        public string PharmacyName { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(100)]
        public string LicenseNumber { get; set; }

        [MaxLength(500)]
        public string PharmacyLicenseDocument { get; set; }

        [MaxLength(500)]
        public string OwnerNationalIdFront { get; set; }

        [MaxLength(500)]
        public string OwnerNationalIdBack { get; set; }

        [MaxLength(500)]
        public string SelfieWithId { get; set; }

        [MaxLength(500)]
        public string SyndicateCard { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public string AdminNotes { get; set; }

        public string RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
