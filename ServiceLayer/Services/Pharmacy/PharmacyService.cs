using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Pharmacy;
using CoreLayer.Specifications.Pharmacy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Pharmacy
{
    public class PharmacyService : IPharmacyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public PharmacyService(
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

        // ==================== PROFILE MANAGEMENT ====================

        public async Task<PharmacyProfileResponseDto> GetPharmacyProfileAsync(string userId)
        {
            var profile = (await _unitOfWork.Repository<PharmacyProfile, Guid>()
                .FindAsync(pp => pp.UserId == userId && pp.IsActive)).FirstOrDefault();

            if (profile == null)
                throw new KeyNotFoundException("Pharmacy profile not found");

            return _mapper.Map<PharmacyProfileResponseDto>(profile);
        }

        public async Task<PharmacyProfileOperationResponseDto> UpdatePharmacyProfileAsync(string userId, UpdatePharmacyProfileDto dto)
        {
            var profile = (await _unitOfWork.Repository<PharmacyProfile, Guid>()
                .FindAsync(pp => pp.UserId == userId && pp.IsActive)).FirstOrDefault();

            if (profile == null)
                throw new KeyNotFoundException("Pharmacy profile not found");

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.PharmacyName)) profile.PharmacyName = dto.PharmacyName;
            if (!string.IsNullOrEmpty(dto.Address)) profile.Address = dto.Address;
            if (!string.IsNullOrEmpty(dto.Phone)) profile.Phone = dto.Phone;
            if (!string.IsNullOrEmpty(dto.Description)) profile.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.WorkingHours)) profile.WorkingHours = dto.WorkingHours;
            if (!string.IsNullOrEmpty(dto.Specializations)) profile.Specializations = dto.Specializations;

            _unitOfWork.Repository<PharmacyProfile, Guid>().Update(profile);
            await _unitOfWork.CompleteAsync();

            return new PharmacyProfileOperationResponseDto
            {
                Success = true,
                Message = "Pharmacy profile updated successfully"
            };
        }

        public async Task<PharmacyProfileOperationResponseDto> UpdatePharmacyLocationAsync(string userId, UpdatePharmacyLocationDto dto)
        {
            var profile = (await _unitOfWork.Repository<PharmacyProfile, Guid>()
                .FindAsync(pp => pp.UserId == userId && pp.IsActive)).FirstOrDefault();

            if (profile == null)
                throw new KeyNotFoundException("Pharmacy profile not found");

            profile.Latitude = dto.Latitude;
            profile.Longitude = dto.Longitude;

            _unitOfWork.Repository<PharmacyProfile, Guid>().Update(profile);
            await _unitOfWork.CompleteAsync();

            return new PharmacyProfileOperationResponseDto
            {
                Success = true,
                Message = "Location updated successfully"
            };
        }

        public async Task<PharmacyProfileOperationResponseDto> DeletePharmacyAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Delete pharmacy profile
            var profile = (await _unitOfWork.Repository<PharmacyProfile, Guid>()
                .FindAsync(pp => pp.UserId == userId)).FirstOrDefault();

            if (profile != null)
            {
                _unitOfWork.Repository<PharmacyProfile, Guid>().Delete(profile);
            }

            // Delete Pharmacy ratings
            var ratings = await _unitOfWork.Repository<PharmacyRating, int>()
                .FindAsync(dr => dr.PharmacyId == userId);

            foreach (var rating in ratings)
            {
                _unitOfWork.Repository<PharmacyRating, int>().Delete(rating);
            }

            // Soft delete all pharmacy listings (not physical deletion)
            var listings = await _unitOfWork.Repository<PharmacyListing, int>()
                .FindAsync(pl => pl.PharmacyId == userId && pl.IsActive);

            foreach (var listing in listings)
            {
               
                if (!string.IsNullOrEmpty(listing.ImageUrls))
                {
                    var imageNames = listing.ImageUrls.Split(',');
                    foreach (var imageName in imageNames)
                    {
                        DocumentSetting.Delete(imageName.Trim(), "pharmacy-listings");
                    }
                }

                // Soft delete (mark as inactive)
                listing.IsActive = false;

                _unitOfWork.Repository<PharmacyListing, int>().Update(listing);
            }

            // Delete pharmacy application
            var application = (await _unitOfWork.Repository<PharmacyApply, Guid>()
                .FindAsync(pa => pa.UserId == userId)).FirstOrDefault();

            if (application != null)
            {
                // Delete uploaded documents
                if (!string.IsNullOrEmpty(application.PharmacyLicenseDocument))
                    DocumentSetting.Delete(application.PharmacyLicenseDocument, "pharmacy-documents");
                if (!string.IsNullOrEmpty(application.OwnerNationalIdFront))
                    DocumentSetting.Delete(application.OwnerNationalIdFront, "pharmacy-documents");
                if (!string.IsNullOrEmpty(application.OwnerNationalIdBack))
                    DocumentSetting.Delete(application.OwnerNationalIdBack, "pharmacy-documents");
                if (!string.IsNullOrEmpty(application.SelfieWithId))
                    DocumentSetting.Delete(application.SelfieWithId, "pharmacy-documents");
                if (!string.IsNullOrEmpty(application.SyndicateCard))
                    DocumentSetting.Delete(application.SyndicateCard, "pharmacy-documents");

                _unitOfWork.Repository<PharmacyApply, Guid>().Delete(application);
            }

            // Remove Pharmacy role
            var isPharmacy = await _userManager.IsInRoleAsync(user, "Pharmacy");
            if (isPharmacy)
            {
                await _userManager.RemoveFromRoleAsync(user, "Pharmacy");
            }

            await _unitOfWork.CompleteAsync();

            return new PharmacyProfileOperationResponseDto
            {
                Success = true,
                Message = "Pharmacy account deleted successfully. All listings were deactivated and your pharmacy role removed."
            };
        }

        // ==================== RATINGS ====================

        public async Task<PharmacyRatingListDto> GetPharmacyRatingsAsync(string pharmacyId)
        {
            var ratings = await _unitOfWork.Repository<PharmacyRating, int>()
                .FindAsync(pr => pr.PharmacyId == pharmacyId);

            var ratingDtos = new List<PharmacyRatingDto>();

            foreach (var rating in ratings.OrderByDescending(r => r.CreatedAt))
            {
                var user = await _userManager.FindByIdAsync(rating.UserId);
                if (user != null)
                {
                    ratingDtos.Add(new PharmacyRatingDto
                    {
                        Id = rating.Id,
                        UserName = $"{user.FirstName} {user.LastName}",
                        UserProfilePicture = user.ProfilePicture,
                        Rating = rating.Rating,
                        Review = rating.Review,
                        ServiceRating = rating.ServiceRating,
                        ProductAvailabilityRating = rating.ProductAvailabilityRating,
                        PricingRating = rating.PricingRating,
                        LocationRating = rating.LocationRating,
                        CreatedAt = rating.CreatedAt
                    });
                }
            }

            var averageRating = ratingDtos.Any() ? ratingDtos.Average(r => r.Rating) : 0;

            return new PharmacyRatingListDto
            {
                Count = ratingDtos.Count,
                AverageRating = Math.Round(averageRating, 2),
                Data = ratingDtos
            };
        }

        // ==================== PHARMACY LISTINGS (PRODUCTS) ====================

        public async Task<PaginationResponse<PharmacyListingResponseDto>> GetMyListingsAsync(
            string userId,
            PharmacyListingFilterParams filterParams)
        {
            if (filterParams.PageIndex < 1)
                throw new ArgumentException("PageIndex must be greater than 0");

            if (filterParams.PageSize < 1)
                throw new ArgumentException("PageSize must be greater than 0");

            var spec = new PharmacyListingByOwnerSpecification(userId, filterParams);
            var countSpec = new PharmacyListingByOwnerCountSpecification(userId, filterParams);

            // IMPORTANT: Load entities with navigation properties (NOT using ProjectTo) cause image URL needs base URL from config and complex mapping
            var listings = await _unitOfWork.Repository<PharmacyListing, int>()
                .GetAllWithSpecficationAsync(spec);

            // Map in-memory - this allows complex transformations
            var listingDtos = _mapper.Map<IEnumerable<PharmacyListingResponseDto>>(listings);

            var totalCount = await _unitOfWork.Repository<PharmacyListing, int>()
                .GetCountAsync(countSpec);

            return new PaginationResponse<PharmacyListingResponseDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                listingDtos
            );
        }

        public async Task<PharmacyListingResponseDto> GetMyListingByIdAsync(int id, string userId)
        {
            var spec = new PharmacyListingByIdSpecification(id);
            var listing = await _unitOfWork.Repository<PharmacyListing, int>()
                .GetWithSpecficationAsync(spec);

            if (listing == null)
                throw new KeyNotFoundException("Listing not found");

            if (listing.PharmacyId != userId)
                throw new UnauthorizedAccessException("You don't have permission to access this listing");

            return _mapper.Map<PharmacyListingResponseDto>(listing);
        }

        public async Task<PharmacyListingOperationResponseDto> AddListingAsync(AddPharmacyListingDto dto, string userId)
        {
            // Verify user is a pharmacy
            var profile = (await _unitOfWork.Repository<PharmacyProfile, Guid>()
                .FindAsync(pp => pp.UserId == userId && pp.IsActive)).FirstOrDefault();

            if (profile == null)
                throw new KeyNotFoundException("Pharmacy profile not found");

            // Validate species if provided
            if (dto.SpeciesId.HasValue)
            {
                var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId.Value);
                if (species == null || !species.IsActive)
                    throw new KeyNotFoundException("Species not found or inactive");
            }

            // Upload images
            var imageUrls = new List<string>();
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var image in dto.Images)
                {
                    var imageName = DocumentSetting.Upload(image, "pharmacy-listings");
                    imageUrls.Add(imageName);
                }
            }

            var listing = new PharmacyListing
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                IsActive = true,
                ImageUrls = string.Join(",", imageUrls),
                PharmacyId = userId,
                SpeciesId = dto.SpeciesId,
                Category = dto.Category,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<PharmacyListing, int>().AddAsync(listing);
            await _unitOfWork.CompleteAsync();

            return new PharmacyListingOperationResponseDto
            {
                Success = true,
                Message = "Product listing created successfully",
                ListingId = listing.Id
            };
        }

        public async Task<PharmacyListingOperationResponseDto> UpdateListingAsync(int id, UpdatePharmacyListingDto dto, string userId)
        {
            var listing = await _unitOfWork.Repository<PharmacyListing, int>().GetAsync(id);
            if (listing == null)
                throw new KeyNotFoundException("Listing not found");

            if (listing.PharmacyId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this listing");

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.Title)) listing.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description)) listing.Description = dto.Description;
            if (dto.Price.HasValue) listing.Price = dto.Price.Value;
            if (dto.Stock.HasValue) listing.Stock = dto.Stock.Value;
            if (dto.Category != null) listing.Category = dto.Category.Value;
            if (dto.SpeciesId.HasValue) listing.SpeciesId = dto.SpeciesId.Value;

            // Handle image updates
            if (dto.Images != null && dto.Images.Any())
            {
                // Delete old images
                if (!string.IsNullOrEmpty(listing.ImageUrls))
                {
                    var oldImageNames = listing.ImageUrls.Split(',');
                    foreach (var imageName in oldImageNames)
                    {
                        DocumentSetting.Delete(imageName.Trim(), "pharmacy-listings");
                    }
                }

                // Upload new images
                var imageUrls = new List<string>();
                foreach (var image in dto.Images)
                {
                    var imageName = DocumentSetting.Upload(image, "pharmacy-listings");
                    imageUrls.Add(imageName);
                }
                listing.ImageUrls = string.Join(",", imageUrls);
            }

            _unitOfWork.Repository<PharmacyListing, int>().Update(listing);
            await _unitOfWork.CompleteAsync();

            return new PharmacyListingOperationResponseDto
            {
                Success = true,
                Message = "Listing updated successfully",
                ListingId = id
            };
        }

        public async Task<PharmacyListingOperationResponseDto> DeleteListingAsync(int id, string userId)
        {
            var listing = await _unitOfWork.Repository<PharmacyListing, int>().GetAsync(id);
            if (listing == null)
                throw new KeyNotFoundException("Listing not found");

            if (listing.PharmacyId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this listing");

            // Delete images
            if (!string.IsNullOrEmpty(listing.ImageUrls))
            {
                var imageNames = listing.ImageUrls.Split(',');
                foreach (var imageName in imageNames)
                {
                    DocumentSetting.Delete(imageName.Trim(), "pharmacy-listings");
                }
            }

            // Soft delete
            listing.IsActive = false;
            _unitOfWork.Repository<PharmacyListing, int>().Update(listing);
            await _unitOfWork.CompleteAsync();

            return new PharmacyListingOperationResponseDto
            {
                Success = true,
                Message = "Listing deleted successfully",
                ListingId = id
            };
        }

        public async Task<PharmacyListingOperationResponseDto> UpdateListingStockAsync(int id, UpdateListingStockDto dto, string userId)
        {
            var listing = await _unitOfWork.Repository<PharmacyListing, int>().GetAsync(id);
            if (listing == null)
                throw new KeyNotFoundException("Listing not found");

            if (listing.PharmacyId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this listing");

            listing.Stock = dto.Stock;

            _unitOfWork.Repository<PharmacyListing, int>().Update(listing);
            await _unitOfWork.CompleteAsync();

            return new PharmacyListingOperationResponseDto
            {
                Success = true,
                Message = "Stock updated successfully",
                ListingId = id
            };
        }
    }
}
