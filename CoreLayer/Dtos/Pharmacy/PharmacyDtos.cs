using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CoreLayer.Dtos.Pharmacy
{
    // ==================== INPUT DTOs ====================

    public class PharmacyFilterParams
    {
        public string? City { get; set; }
        public string? Government { get; set; }
        public string? Search { get; set; }
        public string? Specialization { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PharmacyListingFilterParams
    {
        public int? SpeciesId { get; set; }
        public string? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ApplyPharmacyDto
    {
        [Required]
        [MaxLength(200)]
        public string PharmacyName { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; }

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Required]
        [MaxLength(100)]
        public string LicenseNumber { get; set; }

        [Required]
        public IFormFile PharmacyLicenseDocument { get; set; }

        [Required]
        public IFormFile OwnerNationalIdFront { get; set; }

        [Required]
        public IFormFile OwnerNationalIdBack { get; set; }

        [Required]
        public IFormFile SelfieWithId { get; set; }

        [Required]
        public IFormFile SyndicateCard { get; set; }
    }

    public class ReviewPharmacyApplicationDto
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // "Approved" or "Rejected"

        public string? RejectionReason { get; set; }
    }

    public class UpdatePharmacyLocationDto
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }

    public class UpdatePharmacyProfileDto
    {
        [MaxLength(200)]
        public string? PharmacyName { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public string? Description { get; set; }

        public string? WorkingHours { get; set; }

        public string? Specializations { get; set; }
    }

    public class RatePharmacyDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Review { get; set; }

        [Required]
        [Range(1, 5)]
        public int ServiceRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int ProductAvailabilityRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int PricingRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int LocationRating { get; set; }
    }

    public class AddPharmacyListingDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public List<IFormFile>? Images { get; set; }

        public int? SpeciesId { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }
    }

    public class UpdatePharmacyListingDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        public List<IFormFile>? Images { get; set; }

        public int? SpeciesId { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }
    }

    public class UpdateListingStockDto
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }

    // ==================== OUTPUT DTOs ====================

    public class PharmacyApplicationListDto
    {
        public int Count { get; set; }
        public IEnumerable<PharmacyApplicationSummaryDto> Data { get; set; }
    }

    public class PharmacyApplicationSummaryDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        public string PharmacyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string LicenseNumber { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class PharmacyApplicationDetailDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        public string PharmacyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string LicenseNumber { get; set; }
        public string PharmacyLicenseDocument { get; set; }
        public string OwnerNationalIdFront { get; set; }
        public string OwnerNationalIdBack { get; set; }
        public string SelfieWithId { get; set; }
        public string SyndicateCard { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? AdminNotes { get; set; }
    }

    public class UserPharmacyApplicationStatusDto
    {
        public Guid ApplicationId { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class PharmacyProfileResponseDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string PharmacyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public string WorkingHours { get; set; }
        public bool IsActive { get; set; }
        public string Specializations { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PublicPharmacyProfileDto
    {
        public Guid Id { get; set; }
        public string PharmacyName { get; set; }
        public string OwnerName { get; set; }
        public string ProfilePicture { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public string WorkingHours { get; set; }
        public string Specializations { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public string City { get; set; }
        public string Government { get; set; }
        public IEnumerable<PharmacyRatingDto> RecentRatings { get; set; }
    }

    public class PharmacyRatingListDto
    {
        public int Count { get; set; }
        public double AverageRating { get; set; }
        public IEnumerable<PharmacyRatingDto> Data { get; set; }
    }

    public class PharmacyRatingDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
        public int Rating { get; set; }
        public string Review { get; set; }
        public int ServiceRating { get; set; }
        public int ProductAvailabilityRating { get; set; }
        public int PricingRating { get; set; }
        public int LocationRating { get; set; }
        public bool IsVerifiedExperience { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PharmacyListingResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public List<string> ImageUrls { get; set; }
        public string PharmacyId { get; set; }
        public string PharmacyName { get; set; }
        public int? SpeciesId { get; set; }
        public string? SpeciesName { get; set; }
        public string? Category { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PharmacyApplicationOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid? ApplicationId { get; set; }
    }

    public class PharmacyProfileOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class PharmacyListingOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ListingId { get; set; }
    }
}
