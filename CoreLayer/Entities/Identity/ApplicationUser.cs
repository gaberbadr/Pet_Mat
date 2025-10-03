using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Accessories;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Carts;
using CoreLayer.Entities.Community;
using CoreLayer.Entities.Doctors;
using CoreLayer.Entities.Messages;
using CoreLayer.Entities.Orders;
using CoreLayer.Entities.Pharmacies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace CoreLayer.Entities.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(500)]
        public string? ProfilePicture { get; set; }

        public bool IsActive { get; set; } = true;

        public int? AddressId { get; set; }

        [MaxLength(10)]
        public string? VerificationCode { get; set; }

        public DateTime? CodeExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool HasPasswordAsync { get; set; } = false;

        // Navigation Properties
        public Address Address { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public ICollection<Animal> Animals { get; set; }
        public ICollection<AnimalListing> AnimalListings { get; set; }
        public DoctorApply DoctorApplication { get; set; }
        public DoctorProfile DoctorProfile { get; set; }
        public PharmacyApply PharmacyApplication { get; set; }
        public PharmacyProfile PharmacyProfile { get; set; }
        public ICollection<DoctorRating> DoctorRatingsGiven { get; set; }
        public ICollection<PharmacyRating> PharmacyRatingsGiven { get; set; }
        public Cart Cart { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<AccessoryListing> AccessoryListings { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<PostReaction> PostReactions { get; set; }
        public ICollection<Message> SentMessages { get; set; }
        public ICollection<Message> ReceivedMessages { get; set; }
        public ICollection<UserConnection> UserConnections { get; set; }
        public ICollection<UserBlock> BlocksInitiated { get; set; }
        public ICollection<UserBlock> BlocksReceived { get; set; }
    }
}
