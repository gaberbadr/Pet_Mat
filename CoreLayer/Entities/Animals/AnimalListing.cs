using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreLayer.Entities.Animals
{
    public class AnimalListing : BaseEntity<int>
    {

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string Type { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public int AnimalId { get; set; }

        public bool IsActive { get; set; } = true;
        public string OwnerId { get; set; }

        public string? ExtraPropertiesJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("AnimalId")]
        public Animal Animal { get; set; }

        [ForeignKey("OwnerId")]
        public ApplicationUser Owner { get; set; }
    }
}
