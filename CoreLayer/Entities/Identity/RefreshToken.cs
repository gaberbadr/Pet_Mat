using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Entities.Identity
{
    public class RefreshToken : BaseEntity<int>
    {

        [Required]
        public string Token { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;


        [MaxLength(50)]
        public string CreatedByIp { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
