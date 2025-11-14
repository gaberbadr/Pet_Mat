using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Animals;
using CoreLayer.Dtos.Products;
using CoreLayer.Entities.Foods;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Products;
using Microsoft.Extensions.Configuration;
using static CoreLayer.Specifications.Products.ProductsSpecification;

namespace ServiceLayer.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<PaginationResponse<ProductDto>> GetAllProductsAsync(ProductFilterParams filterParams)
        {
            if (filterParams.PageIndex < 1)
                throw new ArgumentException("PageIndex must be greater than 0");

            if (filterParams.PageSize < 1)
                throw new ArgumentException("PageSize must be greater than 0");

            var spec = new ProductFilterSpecification(filterParams);
            var countSpec = new ProductCountSpecification(filterParams);

            var products = await _unitOfWork.Repository<Product, int>()
                .GetAllWithSpecficationAsync(spec);

            var totalCount = await _unitOfWork.Repository<Product, int>()
                .GetCountAsync(countSpec);

            var productDtos = products.Select(p => MapProductToDto(p)).ToList();

            return new PaginationResponse<ProductDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                productDtos
            );
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var spec = new ProductByIdSpecification(id);
            var product = await _unitOfWork.Repository<Product, int>()
                .GetWithSpecficationAsync(spec);

            if (product == null || !product.IsActive)
                throw new KeyNotFoundException("Product not found or inactive");

            return MapProductToDto(product);
        }

        public async Task<ProductBrandListDto> GetAllBrandsAsync()
        {
            var brands = await _unitOfWork.Repository<ProductBrand, int>().GetAllAsync();
            var brandDtos = brands.Select(b => MapBrandToDto(b)).ToList();

            return new ProductBrandListDto
            {
                Count = brandDtos.Count,
                Data = brandDtos
            };
        }

        public async Task<ProductTypeListDto> GetAllTypesAsync()
        {
            var types = await _unitOfWork.Repository<ProductType, int>().GetAllAsync();
            var typeDtos = _mapper.Map<IEnumerable<ProductTypeDto>>(types);

            return new ProductTypeListDto
            {
                Count = typeDtos.Count(),
                Data = typeDtos
            };
        }

        private ProductDto MapProductToDto(Product product)
        {
            var baseUrl = _configuration["BaseURL"];

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PictureUrl = !string.IsNullOrEmpty(product.PictureUrl)
                    ? DocumentSetting.GetFileUrl(product.PictureUrl, "products", baseUrl)
                    : null,
                Price = product.Price,
                Stock = product.Stock,
                IsActive = product.IsActive,
                NutritionalInfo = product.NutritionalInfo,
                Ingredients = product.Ingredients,
                ExpiryDate = product.ExpiryDate,
                CreatedAt = product.CreatedAt,
                Brand = MapBrandToDto(product.Brand),
                Type = _mapper.Map<ProductTypeDto>(product.Type),
                Species = product.Species != null ? _mapper.Map<SpeciesDto>(product.Species) : null
            };
        }

        private ProductBrandDto MapBrandToDto(ProductBrand brand)
        {
            var baseUrl = _configuration["BaseURL"];

            return new ProductBrandDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = !string.IsNullOrEmpty(brand.LogoUrl)
                    ? DocumentSetting.GetFileUrl(brand.LogoUrl, "brands", baseUrl)
                    : null,
                CreatedAt = brand.CreatedAt
            };
        }
    }
}
