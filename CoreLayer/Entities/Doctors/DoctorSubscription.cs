using System;
using CoreLayer.Enums;
using CoreLayer.Entities;

namespace CoreLayer.Entities.Doctors
{
    public class DoctorSubscription : BaseEntity<int>
    {
        public Guid DoctorId { get; set; }                    // FK → DoctorProfile (AppUser Id)
        public DoctorProfile DoctorProfile { get; set; }

        public int PackageId { get; set; }
        public SubscriptionPackage Package { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;

        // Stripe
        public string PaymentIntentId { get; set; }
        public string ClientSecret { get; set; }

        public decimal AmountPaid { get; set; }
    }
}
