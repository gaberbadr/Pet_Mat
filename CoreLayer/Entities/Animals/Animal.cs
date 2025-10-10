using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;

namespace CoreLayer.Entities.Animals
{
    public class Animal : BaseEntity<int>
    {

        [MaxLength(100)]
        public string PetName { get; set; }

        public int SpeciesId { get; set; }

        public int? SubSpeciesId { get; set; }

        public int? ColorId { get; set; }

        [MaxLength(50)]
        public string Age { get; set; }

        [MaxLength(50)]
        public string Size { get; set; }

        [MaxLength(20)]
        public string Gender { get; set; }

        [Required]
        public string OwnerId { get; set; }

        public string Description { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public string? ExtraPropertiesJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("SpeciesId")]
        public Species Species { get; set; }

        [ForeignKey("SubSpeciesId")]
        public SubSpecies SubSpecies { get; set; }

        [ForeignKey("ColorId")]
        public Color Color { get; set; }

        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; }

        public ICollection<AnimalListing> Listings { get; set; }
    }
}
