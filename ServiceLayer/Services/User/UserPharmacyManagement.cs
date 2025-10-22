using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.User;
using CoreLayer.Specifications.Pharmacy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.User
{
    public class UserPharmacyManagement : IUserPharmacyManagement
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserPharmacyManagement(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
        }

        // ==================== PHARMACY ====================

        // Get all pharmacies with filtering and pagination
        public async Task<PaginationResponse<PublicPharmacyProfileDto>> GetPharmaciesAsync(PharmacyFilterParams filterParams)
        {
            // Build specifications
            var spec = new PharmacyFilterSpecification(filterParams);
            var countSpec = new PharmacyFilterCountSpecification(filterParams);

            // Get total count
            var totalCount = await _unitOfWork.Repository<PharmacyProfile, Guid>().GetCountAsync(countSpec);

            // Get filtered + paginated pharmacies
            var pharmacies = await _unitOfWork.Repository<PharmacyProfile, Guid>().GetAllWithSpecficationAsync(spec);

            // Map to DTOs
            var pharmacyDtos = pharmacies.Select(pp => new PublicPharmacyProfileDto
            {
                Id = pp.Id,
                PharmacyName = pp.PharmacyName,
                OwnerName = $"{pp.User.FirstName} {pp.User.LastName}",
                ProfilePicture = pp.User.ProfilePicture,
                Latitude = pp.Latitude,
                Longitude = pp.Longitude,
                Address = pp.Address,
                Phone = pp.Phone,
                Description = pp.Description,
                WorkingHours = pp.WorkingHours,
                Specializations = pp.Specializations,
                AverageRating = pp.AverageRating,
                TotalRatings = pp.TotalRatings,
                City = pp.User?.Address?.City,
                Government = pp.User?.Address?.Government
            }).ToList();

            // Return paginated response
            return new PaginationResponse<PublicPharmacyProfileDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                pharmacyDtos
            );
        }

        public async Task<PublicPharmacyProfileDto> GetPublicPharmacyProfileAsync(string pharmacyId)
        {
            // Verify pharmacy exists
            var pharmacy = await _userManager.FindByIdAsync(pharmacyId);
            if (pharmacy == null)
                throw new KeyNotFoundException("Pharmacy not found");

            var isPharmacy = await _userManager.IsInRoleAsync(pharmacy, "Pharmacy");
            if (!isPharmacy)
                throw new KeyNotFoundException("User is not a pharmacy");

            // Use specification to get pharmacy profile with User and Ratings included
            var spec = new PharmacyByUserIdSpecification(pharmacyId);
            var profile = await _unitOfWork.Repository<PharmacyProfile, Guid>().GetWithSpecficationAsync(spec);

            if (profile == null)
                throw new KeyNotFoundException("Pharmacy profile not found");

            // Get recent ratings
            var recentRatings = profile.Ratings
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r =>
                {
                    var user = _userManager.FindByIdAsync(r.UserId).Result;
                    return new PharmacyRatingDto
                    {
                        Id = r.Id,
                        UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                        UserProfilePicture = user?.ProfilePicture,
                        Rating = r.Rating,
                        Review = r.Review,
                        ServiceRating = r.ServiceRating,
                        ProductAvailabilityRating = r.ProductAvailabilityRating,
                        PricingRating = r.PricingRating,
                        LocationRating = r.LocationRating,
                        CreatedAt = r.CreatedAt
                    };
                }).ToList();

            return new PublicPharmacyProfileDto
            {
                Id = profile.Id,
                PharmacyName = profile.PharmacyName,
                OwnerName = $"{pharmacy.FirstName} {pharmacy.LastName}",
                ProfilePicture = pharmacy.ProfilePicture,
                Latitude = profile.Latitude,
                Longitude = profile.Longitude,
                Address = profile.Address,
                Phone = profile.Phone,
                Description = profile.Description,
                WorkingHours = profile.WorkingHours,
                Specializations = profile.Specializations,
                AverageRating = profile.AverageRating,
                TotalRatings = profile.TotalRatings,
                City = profile.User?.Address?.City,
                Government = profile.User?.Address?.Government,
                RecentRatings = recentRatings
            };
        }

        // ==================== PHARMACY LISTINGS ====================

        public async Task<PaginationResponse<PharmacyListingResponseDto>> GetAllPharmacyListingsAsync(PharmacyListingFilterParams filterParams)
        {
            if (filterParams.PageIndex < 1)
                throw new ArgumentException("PageIndex must be greater than 0");

            if (filterParams.PageSize < 1)
                throw new ArgumentException("PageSize must be greater than 0");

            var spec = new PharmacyListingFilterSpecification(filterParams);
            var countSpec = new PharmacyListingCountSpecification(filterParams);

            var listingDtos = await _unitOfWork.Repository<PharmacyListing, int>()
                .GetAllWithProjectionAsync<PharmacyListingResponseDto>(spec, _mapper.ConfigurationProvider);

            var totalCount = await _unitOfWork.Repository<PharmacyListing, int>()
                .GetCountAsync(countSpec);

            return new PaginationResponse<PharmacyListingResponseDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                listingDtos
            );
        }

        public async Task<PharmacyListingResponseDto> GetPharmacyListingByIdAsync(int id)
        {
            var spec = new PharmacyListingByIdSpecification(id);
            var listing = await _unitOfWork.Repository<PharmacyListing, int>()
                .GetWithSpecficationAsync(spec);

            if (listing == null || !listing.IsActive)
                throw new KeyNotFoundException("Listing not found");

            return _mapper.Map<PharmacyListingResponseDto>(listing);
        }

        public async Task<PaginationResponse<PharmacyListingResponseDto>> GetListingsByPharmacyIdAsync(string pharmacyId, PharmacyListingFilterParams filterParams)
        {
            if (filterParams.PageIndex < 1)
                throw new ArgumentException("PageIndex must be greater than 0");

            if (filterParams.PageSize < 1)
                throw new ArgumentException("PageSize must be greater than 0");

            // Add filter to only show active listings for public view
            var modifiedParams = new PharmacyListingFilterParams
            {
                SpeciesId = filterParams.SpeciesId,
                Category = filterParams.Category,
                MinPrice = filterParams.MinPrice,
                MaxPrice = filterParams.MaxPrice,
                InStock = filterParams.InStock,
                Search = filterParams.Search,
                PageIndex = filterParams.PageIndex,
                PageSize = filterParams.PageSize
            };

            var spec = new PharmacyListingByOwnerSpecification(pharmacyId, modifiedParams);
            var countSpec = new PharmacyListingByOwnerCountSpecification(pharmacyId, modifiedParams);

            var listingDtos = await _unitOfWork.Repository<PharmacyListing, int>()
                .GetAllWithProjectionAsync<PharmacyListingResponseDto>(spec, _mapper.ConfigurationProvider);

            var totalCount = await _unitOfWork.Repository<PharmacyListing, int>()
                .GetCountAsync(countSpec);

            return new PaginationResponse<PharmacyListingResponseDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                listingDtos
            );
        }

        // ==================== PHARMACY APPLICATION (USER) ====================

        public async Task<PharmacyApplicationOperationResponseDto> ApplyToBePharmacyAsync(ApplyPharmacyDto dto, string userId)
        {
            // Check if user already has a role
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Any())
                throw new InvalidOperationException("You already have a role and cannot apply to be a pharmacy");

            // Delete any existing application
            var existingApplication = (await _unitOfWork.Repository<PharmacyApply, Guid>()
                .FindAsync(pa => pa.UserId == userId)).FirstOrDefault();

            if (existingApplication != null)
            {
                // Delete old documents
                if (!string.IsNullOrEmpty(existingApplication.PharmacyLicenseDocument))
                    DocumentSetting.Delete(existingApplication.PharmacyLicenseDocument, "pharmacy-documents");
                if (!string.IsNullOrEmpty(existingApplication.OwnerNationalIdFront))
                    DocumentSetting.Delete(existingApplication.OwnerNationalIdFront, "pharmacy-documents");
                if (!string.IsNullOrEmpty(existingApplication.OwnerNationalIdBack))
                    DocumentSetting.Delete(existingApplication.OwnerNationalIdBack, "pharmacy-documents");
                if (!string.IsNullOrEmpty(existingApplication.SelfieWithId))
                    DocumentSetting.Delete(existingApplication.SelfieWithId, "pharmacy-documents");
                if (!string.IsNullOrEmpty(existingApplication.SyndicateCard))
                    DocumentSetting.Delete(existingApplication.SyndicateCard, "pharmacy-documents");

                _unitOfWork.Repository<PharmacyApply, Guid>().Delete(existingApplication);
                await _unitOfWork.CompleteAsync();
            }

            // Upload new documents
            var pharmacyLicenseDocument = DocumentSetting.Upload(dto.PharmacyLicenseDocument, "pharmacy-documents");
            var ownerNationalIdFront = DocumentSetting.Upload(dto.OwnerNationalIdFront, "pharmacy-documents");
            var ownerNationalIdBack = DocumentSetting.Upload(dto.OwnerNationalIdBack, "pharmacy-documents");
            var selfieWithId = DocumentSetting.Upload(dto.SelfieWithId, "pharmacy-documents");
            var syndicateCard = DocumentSetting.Upload(dto.SyndicateCard, "pharmacy-documents");

            // Create new application
            var application = new PharmacyApply
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PharmacyName = dto.PharmacyName,
                Address = dto.Address,
                Phone = dto.Phone,
                LicenseNumber = dto.LicenseNumber,
                PharmacyLicenseDocument = pharmacyLicenseDocument,
                OwnerNationalIdFront = ownerNationalIdFront,
                OwnerNationalIdBack = ownerNationalIdBack,
                SelfieWithId = selfieWithId,
                SyndicateCard = syndicateCard,
                Status = "Pending",
                AppliedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PharmacyApply, Guid>().AddAsync(application);
            await _unitOfWork.CompleteAsync();

            return new PharmacyApplicationOperationResponseDto
            {
                Success = true,
                Message = "Pharmacy application submitted successfully",
                ApplicationId = application.Id
            };
        }

        public async Task<UserPharmacyApplicationStatusDto> GetPharmacyApplicationStatusAsync(string userId)
        {
            // Check if user already has pharmacy role
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var isPharmacy = await _userManager.IsInRoleAsync(user, "Pharmacy");
            if (isPharmacy)
                throw new InvalidOperationException("You are already a pharmacy and cannot view application status");

            // Get application
            var application = (await _unitOfWork.Repository<PharmacyApply, Guid>()
                .FindAsync(pa => pa.UserId == userId)).FirstOrDefault();

            if (application == null)
                throw new KeyNotFoundException("No pharmacy application found");

            return new UserPharmacyApplicationStatusDto
            {
                ApplicationId = application.Id,
                Status = application.Status,
                AppliedAt = application.AppliedAt,
                RejectionReason = application.RejectionReason
            };
        }

        // ==================== PHARMACY RATING (USER) ====================

        public async Task<RatingOperationResponseDto> RatePharmacyAsync(string pharmacyId, RatePharmacyDto dto, string userId)
        {
            // Verify pharmacy exists and has pharmacy role
            var pharmacy = await _userManager.FindByIdAsync(pharmacyId);
            if (pharmacy == null)
                throw new KeyNotFoundException("Pharmacy not found");

            var isPharmacy = await _userManager.IsInRoleAsync(pharmacy, "Pharmacy");
            if (!isPharmacy)
                throw new KeyNotFoundException("User is not a pharmacy");

            // Check if user already rated this pharmacy
            var existingRating = (await _unitOfWork.Repository<PharmacyRating, int>()
                .FindAsync(pr => pr.PharmacyId == pharmacyId && pr.UserId == userId)).FirstOrDefault();

            if (existingRating != null)
                throw new InvalidOperationException("You have already rated this pharmacy. Use update endpoint to modify your rating.");

            // Create rating
            var rating = new PharmacyRating
            {
                PharmacyId = pharmacyId,
                UserId = userId,
                Rating = dto.Rating,
                Review = dto.Review,
                ServiceRating = dto.ServiceRating,
                ProductAvailabilityRating = dto.ProductAvailabilityRating,
                PricingRating = dto.PricingRating,
                LocationRating = dto.LocationRating,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PharmacyRating, int>().AddAsync(rating);

            // Update pharmacy profile average rating
            var profile = (await _unitOfWork.Repository<PharmacyProfile, Guid>()
                .FindAsync(pp => pp.UserId == pharmacyId && pp.IsActive)).FirstOrDefault();

            if (profile != null)
            {
                var allRatings = await _unitOfWork.Repository<PharmacyRating, int>()
                    .FindAsync(pr => pr.PharmacyId == pharmacyId);

                var ratingsList = allRatings.ToList();
                ratingsList.Add(rating); // Include the new rating

                profile.TotalRatings = ratingsList.Count;
                profile.AverageRating = Math.Round(ratingsList.Average(r => r.Rating), 2);

                _unitOfWork.Repository<PharmacyProfile, Guid>().Update(profile);
            }

            await _unitOfWork.CompleteAsync();

            return new RatingOperationResponseDto
            {
                Success = true,
                Message = "Pharmacy rated successfully",
                RatingId = rating.Id
            };
        }

        public async Task<RatingOperationResponseDto> UpdatePharmacyRatingAsync(string pharmacyId, RatePharmacyDto dto, string userId)
        {
            // Get existing rating
            var rating = (await _unitOfWork.Repository<PharmacyRating, int>()
                .FindAsync(pr => pr.PharmacyId == pharmacyId && pr.UserId == userId)).FirstOrDefault();

            if (rating == null)
                throw new KeyNotFoundException("Rating not found");

            if (rating.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own ratings");

            // Update rating
            rating.Rating = dto.Rating;
            rating.Review = dto.Review;
            rating.ServiceRating = dto.ServiceRating;
            rating.ProductAvailabilityRating = dto.ProductAvailabilityRating;
            rating.PricingRating = dto.PricingRating;
            rating.LocationRating = dto.LocationRating;

            _unitOfWork.Repository<PharmacyRating, int>().Update(rating);

            // Update pharmacy profile average rating
            var profile = (await _unitOfWork.Repository<PharmacyProfile, Guid>()
                .FindAsync(pp => pp.UserId == pharmacyId && pp.IsActive)).FirstOrDefault();

            if (profile != null)
            {
                var allRatings = await _unitOfWork.Repository<PharmacyRating, int>()
                    .FindAsync(pr => pr.PharmacyId == pharmacyId);

                var ratingsList = allRatings.ToList();
                profile.AverageRating = Math.Round(ratingsList.Average(r => r.Rating), 2);

                _unitOfWork.Repository<PharmacyProfile, Guid>().Update(profile);
            }

            await _unitOfWork.CompleteAsync();

            return new RatingOperationResponseDto
            {
                Success = true,
                Message = "Rating updated successfully",
                RatingId = rating.Id
            };
        }
    }
}
