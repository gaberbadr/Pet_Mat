using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;

namespace CoreLayer.Entities.Community
{
    public class Comment : BaseEntity<int>
    {

        [Required]
        public string Content { get; set; }

        [Required]
        public string UserId { get; set; }

        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }

        [ForeignKey("ParentCommentId")]
        public Comment ParentComment { get; set; }

        public ICollection<Comment> Replies { get; set; }
    }
}
