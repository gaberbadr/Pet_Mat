using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;

namespace CoreLayer.Entities.Messages
{
    public class UserConnection : BaseEntity<int>
    {

        [Required]
        [MaxLength(200)]
        public string ConnectionId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DisconnectedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
