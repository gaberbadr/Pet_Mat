using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Accessories;
using CoreLayer.Entities.Community;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Pharmacies;
using Microsoft.Extensions.Hosting;

namespace CoreLayer.Entities.Animals
{
    public class Species : BaseEntity<int>
    {

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public ICollection<SubSpecies> SubSpecies { get; set; }
        public ICollection<Animal> Animals { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Product> Products { get; set; }
        public ICollection<AccessoryListing> AccessoryListings { get; set; }
        public ICollection<PharmacyListing> PharmacyListings { get; set; }
    }
}
