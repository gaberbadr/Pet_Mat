using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.User;
using CoreLayer.Enums;
using Microsoft.AspNetCore.Http;

namespace CoreLayer.Dtos.Accessory
{
    // ==================== INPUT DTOs ====================

    public class AddAccessoryListingDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public AccessoryCondition Condition { get; set; }

        [Required]
        public AccessoryCategory Category { get; set; }


        public int? SpeciesId { get; set; }

        public List<IFormFile>? Images { get; set; }
    }

    public class UpdateAccessoryListingDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        public AccessoryCondition? Condition { get; set; }

        public AccessoryCategory? Category { get; set; }

        [MaxLength(200)]
     
        public int? SpeciesId { get; set; }

        public List<IFormFile>? Images { get; set; }
    }

    public class UpdateAccessoryListingStatusDto
    {
        [Required]
        public ListingStatus NewStatus { get; set; }
    }

    public class AccessoryListingFilterParams
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? SpeciesId { get; set; }

        // Change these to strings for API input
        public string? Category { get; set; }
        public string? Condition { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Search { get; set; }
        public string? City { get; set; }
        public string? Government { get; set; }

        // Helper methods to convert to enums from strings to enums for use in specifications
        public AccessoryCategory? GetCategoryEnum()
        {
            if (string.IsNullOrEmpty(Category)) return null;
            return Enum.TryParse<AccessoryCategory>(Category, true, out var result) ? result : null;
        }

        public AccessoryCondition? GetConditionEnum()
        {
            if (string.IsNullOrEmpty(Condition)) return null;
            return Enum.TryParse<AccessoryCondition>(Condition, true, out var result) ? result : null;
        }
    }

    // ==================== OUTPUT DTOs ====================

    public class AccessoryListingResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Condition { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public List<string> ImageUrls { get; set; }
        public DateTime CreatedAt { get; set; }
        public OwnerDto Owner { get; set; }
        public SpeciesDto Species { get; set; }
    }

    public class AccessoryListingListDto
    {
        public int Count { get; set; }
        public IEnumerable<AccessoryListingResponseDto> Data { get; set; }
    }

    public class AccessoryOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ListingId { get; set; }
    }
}
