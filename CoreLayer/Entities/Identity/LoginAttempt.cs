using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Entities.Identity
{
    public class LoginAttempt : BaseEntity<int>
    {
        [Required]
        public string Email { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        public bool IsSuccessful { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; }
    }
}
