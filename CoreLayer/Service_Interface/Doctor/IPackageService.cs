using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Doctor;

namespace CoreLayer.Service_Interface.Doctor
{
    public interface IPackageService
    {
        Task<IEnumerable<PackageDto>> GetAllPackagesAsync(bool includeInactive = false);
        Task<PackageDto> GetPackageByIdAsync(int id);
        Task<PackageDto> CreatePackageAsync(CreatePackageDto dto);
        Task<PackageDto> UpdatePackageAsync(int id, UpdatePackageDto dto);
        Task DeletePackageAsync(int id);
    }

    public interface IDoctorSubscriptionService
    {
        /// <summary>
        /// Creates a Stripe PaymentIntent and a pending DoctorSubscription record.
        /// The doctor pays using the returned ClientSecret.
        /// </summary>
        Task<SubscriptionDto> CreateSubscriptionPaymentAsync(string doctorId, CreateSubscriptionDto dto);

        /// <summary>Called by the Stripe webhook to activate or cancel a subscription.</summary>
        Task UpdateSubscriptionStatusAsync(string paymentIntentId, bool isSuccessful);

        Task<SubscriptionDto> GetActiveSubscriptionAsync(string doctorId);

        /// <summary>Deletes the current active subscription for a doctor.</summary>
        Task DeleteSubscriptionAsync(string doctorId);
    }
}
