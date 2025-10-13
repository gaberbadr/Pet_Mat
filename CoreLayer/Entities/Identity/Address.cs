using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Entities.Identity
{
    public class Address : BaseEntity<int>
    {

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(100)]
        public string Government { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<ApplicationUser> Users { get; set; }
    }
}
