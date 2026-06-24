using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Entities.Doctors;
using CoreLayer.Enums;
using CoreLayer.Service_Interface.Doctor;
using CoreLayer.Specifications.Doctor;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace ServiceLayer.Services.Doctor
{
    public class DoctorSubscriptionService : IDoctorSubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public DoctorSubscriptionService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ─── Create PaymentIntent + Pending Subscription ──────────────────────
        // ─────────────────────────────────────────────────────────────────────

        public async Task<SubscriptionDto> CreateSubscriptionPaymentAsync(string doctorId, CreateSubscriptionDto dto)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(doctorId))
                throw new ArgumentException("DoctorId cannot be null or empty", nameof(doctorId));

            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "CreateSubscriptionDto cannot be null");

            // Parse and validate doctorId as GUID
            Guid doctorGuid;
            if (!Guid.TryParse(doctorId, out doctorGuid))
                throw new ArgumentException("Invalid DoctorId format. Expected a valid GUID.", nameof(doctorId));

            // Configure Stripe API key
            StripeConfiguration.ApiKey = _configuration["Stripe:Secretkey"];
            if (string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
                throw new InvalidOperationException("Stripe secret key not configured");

            // 1. Load subscription package
            var package = await _unitOfWork.Repository<SubscriptionPackage, int>().GetAsync(dto.PackageId);
            if (package == null)
                throw new KeyNotFoundException($"Subscription package not found with ID: {dto.PackageId}");

            if (!package.IsActive)
                throw new InvalidOperationException($"Subscription package (ID: {dto.PackageId}) is inactive");

            // 2. Verify doctor profile exists
            var doctorProfile = await _unitOfWork.Repository<DoctorProfile, Guid>().GetAsync(doctorGuid);
            if (doctorProfile == null)
                throw new KeyNotFoundException($"Doctor profile not found for ID: {doctorId}");

            // 3. Block duplicate active/pending subscriptions
            var activeSpec = new ActiveSubscriptionByDoctorSpecification(doctorGuid);
            var existing = await _unitOfWork.Repository<DoctorSubscription, int>()
                .GetWithSpecficationAsync(activeSpec);
            
            if (existing != null)
            {
                throw new InvalidOperationException(
                    $"Doctor already has an active or pending subscription. Current status: {existing.Status}");
            }

            // 4. Create Stripe PaymentIntent
            var service = new PaymentIntentService();
            PaymentIntent paymentIntent;
            
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)Math.Round(package.Price * 100m), // Convert to cents
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "doctorId", doctorId },
                        { "packageId", package.Id.ToString() },
                        { "packageName", package.Name },
                        { "type", "doctor_subscription" }
                    }
                };
                paymentIntent = await service.CreateAsync(options);
            }
            catch (StripeException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create Stripe PaymentIntent: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unexpected error creating PaymentIntent: {ex.Message}", ex);
            }

            // 5. Validate Stripe response
            if (paymentIntent == null)
                throw new InvalidOperationException("Stripe API returned null PaymentIntent");

            if (string.IsNullOrWhiteSpace(paymentIntent.Id))
                throw new InvalidOperationException("Stripe PaymentIntent ID is null or empty");

            if (string.IsNullOrWhiteSpace(paymentIntent.ClientSecret))
                throw new InvalidOperationException("Stripe ClientSecret is null or empty");

            // 6. Validate ClientSecret length (database max is 500 chars)
            if (paymentIntent.ClientSecret.Length > 500)
            {
                throw new InvalidOperationException(
                    $"Stripe ClientSecret exceeds maximum length (500 chars). Received: {paymentIntent.ClientSecret.Length} chars");
            }

            // 7. Persist pending subscription
            var subscription = new DoctorSubscription
            {
                DoctorId = doctorGuid,
                PackageId = package.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(package.DurationInDays),
                Status = SubscriptionStatus.Pending,
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                AmountPaid = package.Price,
                IsActive = false
            };

            await _unitOfWork.Repository<DoctorSubscription, int>().AddAsync(subscription);
            await _unitOfWork.CompleteAsync();

            return MapToDto(subscription, package, includeSecret: true);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ─── Webhook Handler - Update Subscription Status ─────────────────────
        // ─────────────────────────────────────────────────────────────────────

        public async Task UpdateSubscriptionStatusAsync(string paymentIntentId, bool isSuccessful)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(paymentIntentId))
                throw new ArgumentException("PaymentIntentId cannot be null or empty", nameof(paymentIntentId));

            // Retrieve subscription by PaymentIntent ID (with related entities loaded)
            var spec = new SubscriptionByPaymentIntentSpecification(paymentIntentId);
            var subscription = await _unitOfWork.Repository<DoctorSubscription, int>()
                .GetWithSpecficationAsync(spec);

            if (subscription == null)
            {
                throw new KeyNotFoundException(
                    $"No subscription found for PaymentIntent: {paymentIntentId}");
            }

            if (isSuccessful)
            {
                // Payment succeeded - activate subscription
                if (subscription.Status != SubscriptionStatus.Active)
                {
                    subscription.Status = SubscriptionStatus.Active;
                    subscription.IsActive = true;
                    subscription.StartDate = DateTime.UtcNow;
                    subscription.EndDate = DateTime.UtcNow.AddDays(subscription.Package.DurationInDays);
                    
                    // Update subscription entity
                    _unitOfWork.Repository<DoctorSubscription, int>().Update(subscription);

                    // Update doctor profile to mark as having subscription
                    if (subscription.DoctorProfile != null)
                    {
                        subscription.DoctorProfile.HasSubscription = true;
                        _unitOfWork.Repository<DoctorProfile, Guid>().Update(subscription.DoctorProfile);
                    }

                    await _unitOfWork.CompleteAsync();
                }
            }
            else
            {
                // Payment failed - mark subscription as failed
                if (subscription.Status != SubscriptionStatus.Failed &&
                    subscription.Status != SubscriptionStatus.Cancelled)
                {
                    subscription.Status = SubscriptionStatus.Failed;
                    subscription.IsActive = false;
                    
                    _unitOfWork.Repository<DoctorSubscription, int>().Update(subscription);
                    await _unitOfWork.CompleteAsync();
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ─── Query Methods ────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────

        public async Task<SubscriptionDto> GetActiveSubscriptionAsync(string doctorId)
        {
            if (string.IsNullOrWhiteSpace(doctorId))
                throw new ArgumentException("DoctorId cannot be null or empty", nameof(doctorId));

            if (!Guid.TryParse(doctorId, out var doctorGuid))
                throw new ArgumentException("Invalid DoctorId format. Expected a valid GUID.", nameof(doctorId));

            var spec = new SubscriptionStatusByDoctorSpecification(doctorGuid);
            var sub = await _unitOfWork.Repository<DoctorSubscription, int>()
                .GetWithSpecficationAsync(spec);
            
            return sub == null ? null : MapToDto(sub, sub.Package);
        }


        // ─────────────────────────────────────────────────────────────────────
        // ─── Mapper Method ────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────

        private static SubscriptionDto MapToDto(
            DoctorSubscription subscription, 
            SubscriptionPackage package, 
            bool includeSecret = false)
        {
            if (subscription == null)
                return null;

            return new SubscriptionDto
            {
                Id = subscription.Id,
                DoctorId = subscription.DoctorId,
                PackageId = subscription.PackageId,
                PackageName = package?.Name,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                Status = subscription.Status.ToString(),
                AmountPaid = subscription.AmountPaid,
                PaymentIntentId = subscription.PaymentIntentId,
                ClientSecret = includeSecret ? subscription.ClientSecret : null
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // ─── Update Subscription ──────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────

        public async Task UpdateSubscriptionAsync(string doctorId, UpdateSubscriptionDto dto)
        {
            if (string.IsNullOrWhiteSpace(doctorId))
                throw new ArgumentException("DoctorId cannot be null or empty", nameof(doctorId));

            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "UpdateSubscriptionDto cannot be null");

            if (dto.Id <= 0)
                throw new ArgumentException("Subscription ID must be greater than 0", nameof(dto.Id));

            if (dto.PackageId <= 0)
                throw new ArgumentException("Package ID must be greater than 0", nameof(dto.PackageId));

            // 1. Load existing subscription
            var entity = await _unitOfWork.Repository<DoctorSubscription, int>().GetAsync(dto.Id);
            if (entity == null)
            {
                throw new KeyNotFoundException(
                    $"Subscription not found with ID: {dto.Id}");
            }

            // 2. Verify new package exists and is active
            var newPackage = await _unitOfWork.Repository<SubscriptionPackage, int>().GetAsync(dto.PackageId);
            if (newPackage == null)
            {
                throw new KeyNotFoundException(
                    $"Subscription package not found with ID: {dto.PackageId}");
            }

            if (!newPackage.IsActive)
            {
                throw new InvalidOperationException(
                    $"Cannot change to inactive package. Package ID: {dto.PackageId}");
            }

            // 3. Validate and apply changes
            entity.PackageId = dto.PackageId;
            entity.AmountPaid = dto.AmountPaid;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.Status = dto.Status == "Active" ? SubscriptionStatus.Active : SubscriptionStatus.Pending;

            // 4. Persist changes
            _unitOfWork.Repository<DoctorSubscription, int>().Update(entity);
            await _unitOfWork.CompleteAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // ─── Delete Subscription ───────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────

        public async Task DeleteSubscriptionAsync(string doctorId)
        {
            if (string.IsNullOrWhiteSpace(doctorId))
                throw new ArgumentException("DoctorId cannot be null or empty", nameof(doctorId));

            if (!Guid.TryParse(doctorId, out var doctorGuid))
                throw new ArgumentException("Invalid DoctorId format. Expected a valid GUID.", nameof(doctorId));

            // Find active subscription
            var spec = new ActiveSubscriptionByDoctorSpecification(doctorGuid);
            var subscription = await _unitOfWork.Repository<DoctorSubscription, int>()
                .GetWithSpecficationAsync(spec);

            if (subscription == null)
                throw new KeyNotFoundException("No active subscription found to delete");

            // Delete subscription
            _unitOfWork.Repository<DoctorSubscription, int>().Delete(subscription);

            // Update doctor profile to remove subscription flag
            var doctorProfile = await _unitOfWork.Repository<DoctorProfile, Guid>().GetAsync(doctorGuid);
            if (doctorProfile != null)
            {
                doctorProfile.HasSubscription = false;
                _unitOfWork.Repository<DoctorProfile, Guid>().Update(doctorProfile);
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}
