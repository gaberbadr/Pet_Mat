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
    public class DoctorRating : BaseEntity<int>
    {

        [Required]
        public string DoctorId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Review { get; set; }

        [Range(1, 5)]
        public int CommunicationRating { get; set; }

        [Range(1, 5)]
        public int KnowledgeRating { get; set; }

        [Range(1, 5)]
        public int ResponsivenessRating { get; set; }

        [Range(1, 5)]
        public int ProfessionalismRating { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("DoctorId")]
        public ApplicationUser Doctor { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }

}
