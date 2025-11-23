using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Entities.Identity
{
    public class UserBlock : BaseEntity<int>
    {

        [Required]
        public string BlockerId { get; set; }

        [Required]
        public string BlockedId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UnblockedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("BlockerId")]
        public ApplicationUser Blocker { get; set; }

        [ForeignKey("BlockedId")]
        public ApplicationUser Blocked { get; set; }
    }
}
