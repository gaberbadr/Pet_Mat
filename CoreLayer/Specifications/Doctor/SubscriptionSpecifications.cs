using System;
using System.Linq.Expressions;
using CoreLayer.Entities;
using CoreLayer.Entities.Doctors;
using CoreLayer.Enums;

namespace CoreLayer.Specifications.Doctor
{
    /// <summary>
    /// Specification to retrieve active or pending subscriptions for a doctor.
    /// Includes related DoctorProfile and SubscriptionPackage entities.
    /// </summary>
    public class ActiveSubscriptionByDoctorSpecification : BaseSpecifications<DoctorSubscription, int>
    {
        public ActiveSubscriptionByDoctorSpecification(Guid doctorId)
            : base(ds => ds.DoctorId == doctorId && 
                   (ds.Status == SubscriptionStatus.Active || ds.Status == SubscriptionStatus.Pending))
        {
            // Eagerly load related entities to prevent lazy-loading issues
            Includes.Add(ds => ds.DoctorProfile);
            Includes.Add(ds => ds.Package);
            
            // Order by most recent start date
            OrderByDescending = ds => ds.StartDate;
            
            // Only take the most recent subscription
            applyPagnation(skip: 0, take: 1);
        }
    }

    public class SubscriptionStatusByDoctorSpecification : BaseSpecifications<DoctorSubscription, int>
    {
        public SubscriptionStatusByDoctorSpecification(Guid doctorId)
            : base(ds => ds.DoctorId == doctorId)
        {
            // Eagerly load related entities to prevent lazy-loading issues
            Includes.Add(ds => ds.DoctorProfile);
            Includes.Add(ds => ds.Package);

            // Order by most recent start date
            OrderByDescending = ds => ds.StartDate;

            // Only take the most recent subscription
            applyPagnation(skip: 0, take: 1);
        }
    }

    /// <summary>
    /// Specification to retrieve a subscription by its Stripe PaymentIntent ID.
    /// Includes related DoctorProfile and SubscriptionPackage entities.
    /// </summary>
    public class SubscriptionByPaymentIntentSpecification : BaseSpecifications<DoctorSubscription, int>
    {
        public SubscriptionByPaymentIntentSpecification(string paymentIntentId)
            : base(ds => ds.PaymentIntentId == paymentIntentId)
        {
            // Eagerly load related entities
            Includes.Add(ds => ds.DoctorProfile);
            Includes.Add(ds => ds.Package);
        }
    }

    /// <summary>
    /// Specification to retrieve subscription history (all subscriptions) for a doctor.
    /// Includes related SubscriptionPackage entity.
    /// </summary>
    public class SubscriptionHistoryByDoctorSpecification : BaseSpecifications<DoctorSubscription, int>
    {
        public SubscriptionHistoryByDoctorSpecification(Guid doctorId)
            : base(ds => ds.DoctorId == doctorId)
        {
            // Eagerly load related package data
            Includes.Add(ds => ds.Package);
            
            // Order by most recent first
            OrderByDescending = ds => ds.StartDate;
        }
    }

    /// <summary>
    /// Specification to check if a doctor has any non-expired active subscription.
    /// </summary>
    public class DoctorHasActiveSubscriptionSpecification : BaseSpecifications<DoctorSubscription, int>
    {
        public DoctorHasActiveSubscriptionSpecification(Guid doctorId)
            : base(ds => ds.DoctorId == doctorId && 
                   ds.Status == SubscriptionStatus.Active &&
                   ds.EndDate > DateTime.UtcNow)
        {
            Includes.Add(ds => ds.Package);
        }
    }
}