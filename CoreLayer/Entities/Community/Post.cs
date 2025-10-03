using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;

namespace CoreLayer.Entities.Community
{
    public class Post : BaseEntity<int>
    {

        [Required]
        public string Content { get; set; }

        [Required]
        public string UserId { get; set; }

        public int? SpeciesId { get; set; }

        public string ImageUrls { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [ForeignKey("SpeciesId")]
        public Species Species { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<PostReaction> Reactions { get; set; }
    }

}
