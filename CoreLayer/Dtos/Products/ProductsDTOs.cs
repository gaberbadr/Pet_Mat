using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Animals;
using Microsoft.AspNetCore.Http;

namespace CoreLayer.Dtos.Products
{
    public class AddProductDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public IFormFile PictureFile { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        public int TypeId { get; set; }

        public int? SpeciesId { get; set; }

        public string NutritionalInfo { get; set; }

        public string Ingredients { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }

    public class UpdateProductDto
    {
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        public IFormFile PictureFile { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        public int? BrandId { get; set; }

        public int? TypeId { get; set; }

        public int? SpeciesId { get; set; }

        public string NutritionalInfo { get; set; }

        public string Ingredients { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool? IsActive { get; set; }
    }

    public class ProductFilterParams
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? BrandId { get; set; }
        public int? TypeId { get; set; }
        public int? SpeciesId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Search { get; set; }
        public bool? InStock { get; set; }
        public string SortBy { get; set; } // "price_asc", "price_desc", "name", "newest"
    }

    // ==================== BRAND DTOs ====================

    public class AddProductBrandDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        public IFormFile? LogoFile { get; set; }
    }

    public class UpdateProductBrandDto
    {
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        public IFormFile? LogoFile { get; set; }
    }

    // ==================== TYPE DTOs ====================

    public class AddProductTypeDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class UpdateProductTypeDto
    {
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }
    }

    // ==================== OUTPUT DTOs ====================

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string NutritionalInfo { get; set; }
        public string Ingredients { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public ProductBrandDto Brand { get; set; }
        public ProductTypeDto Type { get; set; }
        public SpeciesDto Species { get; set; }
    }

    public class ProductBrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductListDto
    {
        public int Count { get; set; }
        public IEnumerable<ProductDto> Data { get; set; }
    }

    public class ProductBrandListDto
    {
        public int Count { get; set; }
        public IEnumerable<ProductBrandDto> Data { get; set; }
    }

    public class ProductTypeListDto
    {
        public int Count { get; set; }
        public IEnumerable<ProductTypeDto> Data { get; set; }
    }

    // ==================== RESPONSE DTOs ====================

    public class ProductOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ProductId { get; set; }
    }

    public class BrandOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? BrandId { get; set; }
    }

    public class TypeOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? TypeId { get; set; }
    }
}
