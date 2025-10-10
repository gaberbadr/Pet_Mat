using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Entities.Animals
{
    public class Color : BaseEntity<int>
    {

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public ICollection<Animal> Animals { get; set; }
    }
}
