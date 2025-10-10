using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Entities.Animals
{
    public class SubSpecies : BaseEntity<int>
    {

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public int SpeciesId { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("SpeciesId")]
        public Species Species { get; set; }
        public ICollection<Animal> Animals { get; set; }
    }
}
