using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Enums;
using Microsoft.AspNetCore.Http;

namespace CoreLayer.Dtos.Animals
{
    // ==================== INPUT DTOs ====================

    public class AddAnimalDto
    {
        [Required]
        [MaxLength(100)]
        public string PetName { get; set; }

        [Required]
        public int SpeciesId { get; set; }

        public int? SubSpeciesId { get; set; }

        public int? ColorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Age { get; set; }

        [Required]
        public AnimalSize Size { get; set; }
        [Required]
        public Gender Gender { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public List<IFormFile> Image { get; set; }

        public string? ExtraPropertiesJson { get; set; }
    }

    public class UpdateAnimalDto
    {
        [MaxLength(100)]
        public string? PetName { get; set; }

        public int? SpeciesId { get; set; }

        public int? SubSpeciesId { get; set; }

        public int? ColorId { get; set; }

        [MaxLength(50)]
        public string? Age { get; set; }

        public AnimalSize? Size { get; set; }

        public Gender? Gender { get; set; }

        public string? Description { get; set; }

        public List<IFormFile>? Image { get; set; }

        public string? ExtraPropertiesJson { get; set; }
    }

    public class AddAnimalListingDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public AnimalListingType? Type { get; set; }

        [Required]
        public int AnimalId { get; set; }

        public string? ExtraPropertiesJson { get; set; }
    }

    public class AnimalListingFilterParams
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? SpeciesId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Search { get; set; }
        public string? City { get; set; }
        public string? Government { get; set; }

        public AnimalListingType? GetAnimalListingTypeEnum()
        { 
            // Return null if Type is null or whitespace
            if (string.IsNullOrWhiteSpace(Type))
                return null;

            // Return null if parsing fails (instead of returning a nullable with no value)
            if (Enum.TryParse<AnimalListingType>(Type, true, out var result))
                return result;

            return null; 
        }

        public ListingStatus? GetListingStatusEnum()
        {
            // Return null if status is null or whitespace
            if (string.IsNullOrWhiteSpace(Status))
                return null;

            // Return null if parsing fails (instead of returning a nullable with no value)
            if (Enum.TryParse<ListingStatus>(Status, true, out var result))
                return result;

            return null;
        }

    }

    public class UpdateListingStatusDto
    {
        [Required]
        public ListingStatus NewStatus { get; set; }
    }

    // ==================== OUTPUT DTOs ====================

    public class AnimalResponseDto
    {
        public int Id { get; set; }
        public string PetName { get; set; }
        public string Age { get; set; }
        public AnimalSize Size { get; set; }
        public Gender Gender { get; set; }
        public List<string>? ImageUrl { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public SpeciesDto Species { get; set; }
        public string SubSpeciesName { get; set; }
        public string ColorName { get; set; }
        public string Message { get; set; }
    }

    public class AnimalListDto
    {
        public int Count { get; set; }
        public IEnumerable<AnimalDto> Data { get; set; }
    }


    public class AnimalDto
    {
        public int Id { get; set; }
        public string PetName { get; set; }
        public string Age { get; set; }
        public AnimalSize Size { get; set; }
        public Gender Gender { get; set; }
        public List<string>? ImageUrl { get; set; }
        public string Description { get; set; }
        public SpeciesDto Species { get; set; }
        public string SubSpeciesName { get; set; }
        public string ColorName { get; set; }
    }

    public class SpeciesDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OwnerDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicture { get; set; }
        public string City { get; set; }
        public string Government { get; set; }
    }

    public class AnimalListingResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public AnimalListingType Type { get; set; }
        public ListingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public AnimalDto Animal { get; set; }
        public OwnerDto Owner { get; set; }
    }

    public class AnimalListingListDto
    {
        public int Count { get; set; }
        public IEnumerable<AnimalListingResponseDto> Data { get; set; }
    }

    public class SpeciesListDto
    {
        public int Count { get; set; }
        public IEnumerable<SpeciesInfoDto> Data { get; set; }
    }

    public class SpeciesInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SubSpeciesListDto
    {
        public int Count { get; set; }
        public IEnumerable<SubSpeciesInfoDto> Data { get; set; }
    }

    public class SubSpeciesInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SpeciesId { get; set; }
    }

    public class ColorListDto
    {
        public int Count { get; set; }
        public IEnumerable<ColorInfoDto> Data { get; set; }
    }

    public class ColorInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AnimalOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? AnimalId { get; set; }
    }

    public class ListingOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ListingId { get; set; }
    }
}