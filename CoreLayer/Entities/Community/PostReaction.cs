using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;
using CoreLayer.Enums;

namespace CoreLayer.Entities.Community
{
    public class PostReaction : BaseEntity<int>
    {

        [Required]
        public string UserId { get; set; }

        public int PostId { get; set; }

        [Required]
        public ReactionType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }
    }
}
