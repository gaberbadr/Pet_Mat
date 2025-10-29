using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Accessory;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Entities.Accessories;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Enums;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Accessory;
using CoreLayer.Specifications.Accessory;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Accessory
{
    public class UserAccessoryManagement : IUserAccessoryManagement
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserAccessoryManagement(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _userManager = userManager;
        }

        // ==================== ACCESSORY LISTINGS ====================

        public async Task<PaginationResponse<AccessoryListingResponseDto>> GetAllAccessoryListingsAsync(AccessoryListingFilterParams filterParams)
        {
            if (filterParams.PageIndex < 1)
                throw new ArgumentException("PageIndex must be greater than 0");

            if (filterParams.PageSize < 1)
                throw new ArgumentException("PageSize must be greater than 0");

            var spec = new AccessoryListingFilterSpecification(filterParams);
            var countSpec = new AccessoryListingCountSpecification(filterParams);

            // IMPORTANT: Load entities with navigation properties (NOT using ProjectTo) cause image URL needs base URL from config and complex mapping
            var listings = await _unitOfWork.Repository<AccessoryListing, int>()
                .GetAllWithSpecficationAsync(spec);
            // Map in-memory - this allows complex transformations
            var listingDtos = _mapper.Map<IEnumerable<AccessoryListingResponseDto>>(listings);

            var totalCount = await _unitOfWork.Repository<AccessoryListing, int>()
                .GetCountAsync(countSpec);

            return new PaginationResponse<AccessoryListingResponseDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                listingDtos
            );
        }

        public async Task<AccessoryListingResponseDto> GetAccessoryListingByIdAsync(int id)
        {
            var spec = new AccessoryListingByIdSpecification(id);
            var listing = await _unitOfWork.Repository<AccessoryListing, int>()
                .GetWithSpecficationAsync(spec);

            if (listing == null)
                throw new KeyNotFoundException("Accessory listing not found or inactive");

            return _mapper.Map<AccessoryListingResponseDto>(listing);
        }

        public async Task<AccessoryListingListDto> GetMyAccessoryListingsAsync(string userId)
        {
            var spec = new AccessoryListingWithDetailsByOwnerSpecification(userId);
            // IMPORTANT: Load entities with navigation properties (NOT using ProjectTo) cause image URL needs base URL from config and complex mapping
            var listings = await _unitOfWork.Repository<AccessoryListing, int>()
                .GetAllWithSpecficationAsync(spec);
            // Map in-memory - this allows complex transformations
            var listingDtos = _mapper.Map<IEnumerable<AccessoryListingResponseDto>>(listings);


            return new AccessoryListingListDto
            {
                Count = listingDtos.Count(),
                Data = listingDtos
            };
        }

        public async Task<AccessoryOperationResponseDto> AddAccessoryListingAsync(AddAccessoryListingDto dto, string userId)
        {
            // Validate species if provided
            if (dto.SpeciesId.HasValue)
            {
                var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId.Value);
                if (species == null || species.IsActive == false)
                    throw new KeyNotFoundException("Species not found or inactive");
            }

            // Handle image uploads
            var imageUrls = new List<string>();
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var image in dto.Images)
                {
                    var fileName = DocumentSetting.Upload(image, "accessory-listings");
                    imageUrls.Add(fileName);
                }
            }

            var listing = new AccessoryListing
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Condition = dto.Condition,
                Category = dto.Category,
                ImageUrls =  string.Join(",", imageUrls),
                OwnerId = userId,
                SpeciesId = dto.SpeciesId,
                Status = ListingStatus.Active,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AccessoryListing, int>().AddAsync(listing);
            await _unitOfWork.CompleteAsync();

            return new AccessoryOperationResponseDto
            {
                Success = true,
                Message = "Accessory listing created successfully",
                ListingId = listing.Id
            };
        }

        public async Task<AccessoryOperationResponseDto> UpdateAccessoryListingAsync(int id, UpdateAccessoryListingDto dto, string userId)
        {
            var listing = await _unitOfWork.Repository<AccessoryListing, int>().GetAsync(id);
            if (listing == null || listing.IsActive == false)
                throw new KeyNotFoundException("Accessory listing not found or inactive");

            if (listing.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this listing");

            // Validate species if provided
            if (dto.SpeciesId.HasValue)
            {
                var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId.Value);
                if (species == null || species.IsActive == false)
                    throw new KeyNotFoundException("Species not found or inactive");
                listing.SpeciesId = dto.SpeciesId;
            }

            // Handle image updates
            if (dto.Images != null && dto.Images.Any())
            {
                // Delete old images
                if (!string.IsNullOrEmpty(listing.ImageUrls))
                {
                    var oldImages = listing.ImageUrls.Split(',');
                    foreach (var oldImage in oldImages)
                    {
                        DocumentSetting.Delete(oldImage, "accessory-listings");
                    }
                }

                // Upload new images
                var imageUrls = new List<string>();
                foreach (var image in dto.Images)
                {
                    var fileName = DocumentSetting.Upload(image, "accessory-listings");
                    imageUrls.Add(fileName);
                }
                listing.ImageUrls = string.Join(",", imageUrls);
            }

            // Update other fields
            if (!string.IsNullOrEmpty(dto.Title)) listing.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description)) listing.Description = dto.Description;
            if (dto.Price.HasValue) listing.Price = dto.Price.Value;
            if (dto.Condition.HasValue) listing.Condition = dto.Condition.Value;
            if (dto.Category.HasValue) listing.Category = dto.Category.Value;

            _unitOfWork.Repository<AccessoryListing, int>().Update(listing);
            await _unitOfWork.CompleteAsync();

            return new AccessoryOperationResponseDto
            {
                Success = true,
                Message = "Accessory listing updated successfully",
                ListingId = listing.Id
            };
        }

        public async Task<AccessoryOperationResponseDto> UpdateAccessoryListingStatusAsync(int listingId, string userId, ListingStatus newStatus)
        {
            var listing = await _unitOfWork.Repository<AccessoryListing, int>().GetAsync(listingId);
            if (listing == null)
                throw new KeyNotFoundException("Accessory listing not found");

            if (listing.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this listing");

            // Check if status is already the same
            if (listing.Status == newStatus)
            {
                return new AccessoryOperationResponseDto
                {
                    Success = false,
                    Message = $"Listing status is already '{newStatus}'.",
                    ListingId = listing.Id
                };
            }

            listing.Status = newStatus;
            _unitOfWork.Repository<AccessoryListing, int>().Update(listing);
            await _unitOfWork.CompleteAsync();

            return new AccessoryOperationResponseDto
            {
                Success = true,
                Message = $"Listing status updated to '{newStatus}' successfully",
                ListingId = listing.Id
            };
        }

        public async Task<AccessoryOperationResponseDto> DeleteAccessoryListingAsync(int id, string userId)
        {
            var listing = await _unitOfWork.Repository<AccessoryListing, int>().GetAsync(id);
            if (listing == null || listing.IsActive == false)
                throw new KeyNotFoundException("Accessory listing not found or already deleted");

            if (listing.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this listing");

            // Delete images
            if (!string.IsNullOrEmpty(listing.ImageUrls))
            {
                var images = listing.ImageUrls.Split(',');
                foreach (var image in images)
                {
                    DocumentSetting.Delete(image, "accessory-listings");
                }
            }

            // Soft delete
            listing.IsActive = false;
            _unitOfWork.Repository<AccessoryListing, int>().Update(listing);
            await _unitOfWork.CompleteAsync();

            return new AccessoryOperationResponseDto
            {
                Success = true,
                Message = "Accessory listing deleted successfully",
                ListingId = id
            };
        }
    }
}
