using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;
using CoreLayer.Enums;

namespace CoreLayer.Entities.Messages
{
    public class Message : BaseEntity<int>
    {
        [Required]
        public string Content { get; set; }

        [MaxLength(50)]
        public MessageType Type { get; set; } = MessageType.Text; // ADD DEFAULT

        [MaxLength(500)]
        public string? MediaUrl { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow; 

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [MaxLength(50)]
        public MessageContextType ContextType { get; set; } = MessageContextType.General; 

        public int? ContextId { get; set; }

        public bool IsRead { get; set; } = false; 

        public bool IsDeleted { get; set; } = false; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 

        // Navigation Properties
        [ForeignKey("SenderId")]
        public ApplicationUser Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public ApplicationUser Receiver { get; set; }
    }
}
