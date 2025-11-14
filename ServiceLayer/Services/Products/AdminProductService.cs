using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Products;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Carts;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Orders;
using CoreLayer.Helper.Documents;
using CoreLayer.Service_Interface.Products;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Products
{
    public class AdminProductService : IAdminProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AdminProductService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        // ==================== PRODUCT MANAGEMENT ====================

        public async Task<ProductOperationResponseDto> AddProductAsync(AddProductDto dto)
        {
            // Validate brand
            var brand = await _unitOfWork.Repository<ProductBrand, int>().GetAsync(dto.BrandId);
            if (brand == null)
                throw new KeyNotFoundException("Brand not found");

            // Validate type
            var type = await _unitOfWork.Repository<ProductType, int>().GetAsync(dto.TypeId);
            if (type == null)
                throw new KeyNotFoundException("Product type not found");

            // Validate species if provided
            if (dto.SpeciesId.HasValue)
            {
                var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId.Value);
                if (species == null || !species.IsActive)
                    throw new KeyNotFoundException("Species not found or inactive");
            }

            // Upload image
            string pictureFileName = null;
            if (dto.PictureFile != null)
            {
                pictureFileName = DocumentSetting.Upload(dto.PictureFile, "products");
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                PictureUrl = pictureFileName,
                Price = dto.Price,
                Stock = dto.Stock,
                BrandId = dto.BrandId,
                TypeId = dto.TypeId,
                SpeciesId = dto.SpeciesId,
                NutritionalInfo = dto.NutritionalInfo,
                Ingredients = dto.Ingredients,
                ExpiryDate = dto.ExpiryDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Product, int>().AddAsync(product);
            await _unitOfWork.CompleteAsync();

            return new ProductOperationResponseDto
            {
                Success = true,
                Message = "Product added successfully",
                ProductId = product.Id
            };
        }

        public async Task<ProductOperationResponseDto> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _unitOfWork.Repository<Product, int>().GetAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            // Validate brand if provided
            if (dto.BrandId.HasValue)
            {
                var brand = await _unitOfWork.Repository<ProductBrand, int>().GetAsync(dto.BrandId.Value);
                if (brand == null)
                    throw new KeyNotFoundException("Brand not found");
                product.BrandId = dto.BrandId.Value;
            }

            // Validate type if provided
            if (dto.TypeId.HasValue)
            {
                var type = await _unitOfWork.Repository<ProductType, int>().GetAsync(dto.TypeId.Value);
                if (type == null)
                    throw new KeyNotFoundException("Product type not found");
                product.TypeId = dto.TypeId.Value;
            }

            // Validate species if provided
            if (dto.SpeciesId.HasValue)
            {
                var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId.Value);
                if (species == null || !species.IsActive)
                    throw new KeyNotFoundException("Species not found or inactive");
                product.SpeciesId = dto.SpeciesId;
            }

            // Update image if provided
            if (dto.PictureFile != null)
            {
                // Delete old image
                if (!string.IsNullOrEmpty(product.PictureUrl))
                {
                    DocumentSetting.Delete(product.PictureUrl, "products");
                }

                product.PictureUrl = DocumentSetting.Upload(dto.PictureFile, "products");
            }

            // Update other fields
            if (!string.IsNullOrEmpty(dto.Name)) product.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) product.Description = dto.Description;
            if (dto.Price.HasValue) product.Price = dto.Price.Value;
            if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
            if (!string.IsNullOrEmpty(dto.NutritionalInfo)) product.NutritionalInfo = dto.NutritionalInfo;
            if (!string.IsNullOrEmpty(dto.Ingredients)) product.Ingredients = dto.Ingredients;
            if (dto.ExpiryDate.HasValue) product.ExpiryDate = dto.ExpiryDate;
            if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;

            _unitOfWork.Repository<Product, int>().Update(product);
            await _unitOfWork.CompleteAsync();

            return new ProductOperationResponseDto
            {
                Success = true,
                Message = "Product updated successfully",
                ProductId = id
            };
        }

        public async Task<ProductOperationResponseDto> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product, int>().GetAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            // Check if product is in any cart
            var cartItems = await _unitOfWork.Repository<CartItem, int>()
                .FindAsync(ci => ci.ProductId == id);

            if (cartItems.Any())
                throw new InvalidOperationException($"Cannot delete product. It's in {cartItems.Count()} cart(s)");

            // Check if product is in any order
            var orderItems = await _unitOfWork.Repository<OrderItem, int>()
                .FindAsync(oi => oi.ProductId == id);

            if (orderItems.Any())
                throw new InvalidOperationException($"Cannot delete product. It's in {orderItems.Count()} order(s)");

            // Delete image
            if (!string.IsNullOrEmpty(product.PictureUrl))
            {
                DocumentSetting.Delete(product.PictureUrl, "products");
            }

            _unitOfWork.Repository<Product, int>().Delete(product);
            await _unitOfWork.CompleteAsync();

            return new ProductOperationResponseDto
            {
                Success = true,
                Message = "Product deleted successfully",
                ProductId = id
            };
        }

        public async Task<ProductOperationResponseDto> ActivateProductAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product, int>().GetAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            if (product.IsActive)
            {
                return new ProductOperationResponseDto
                {
                    Success = false,
                    Message = "Product is already active",
                    ProductId = id
                };
            }

            product.IsActive = true;
            _unitOfWork.Repository<Product, int>().Update(product);
            await _unitOfWork.CompleteAsync();

            return new ProductOperationResponseDto
            {
                Success = true,
                Message = "Product activated successfully",
                ProductId = id
            };
        }

        public async Task<ProductOperationResponseDto> DeactivateProductAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product, int>().GetAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            if (!product.IsActive)
            {
                return new ProductOperationResponseDto
                {
                    Success = false,
                    Message = "Product is already inactive",
                    ProductId = id
                };
            }

            product.IsActive = false;
            _unitOfWork.Repository<Product, int>().Update(product);
            await _unitOfWork.CompleteAsync();

            return new ProductOperationResponseDto
            {
                Success = true,
                Message = "Product deactivated successfully",
                ProductId = id
            };
        }

        // ==================== BRAND MANAGEMENT ====================

        public async Task<BrandOperationResponseDto> AddBrandAsync(AddProductBrandDto dto)
        {
            // Check if brand name already exists
            var existing = await _unitOfWork.Repository<ProductBrand, int>()
                .FindAsync(b => b.Name.ToLower() == dto.Name.ToLower());

            if (existing.Any())
                throw new InvalidOperationException("Brand name already exists");

            // Upload logo if provided
            string logoFileName = null;
            if (dto.LogoFile != null)
            {
                logoFileName = DocumentSetting.Upload(dto.LogoFile, "brands");
            }

            var brand = new ProductBrand
            {
                Name = dto.Name,
                Description = dto.Description,
                LogoUrl = logoFileName,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ProductBrand, int>().AddAsync(brand);
            await _unitOfWork.CompleteAsync();

            return new BrandOperationResponseDto
            {
                Success = true,
                Message = "Brand added successfully",
                BrandId = brand.Id
            };
        }

        public async Task<BrandOperationResponseDto> UpdateBrandAsync(int id, UpdateProductBrandDto dto)
        {
            var brand = await _unitOfWork.Repository<ProductBrand, int>().GetAsync(id);
            if (brand == null)
                throw new KeyNotFoundException("Brand not found");

            // Check if new name conflicts with existing brand
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name.ToLower() != brand.Name.ToLower())
            {
                var existing = await _unitOfWork.Repository<ProductBrand, int>()
                    .FindAsync(b => b.Name.ToLower() == dto.Name.ToLower());

                if (existing.Any())
                    throw new InvalidOperationException("Brand name already exists");

                brand.Name = dto.Name;
            }

            // Update logo if provided
            if (dto.LogoFile != null)
            {
                // Delete old logo
                if (!string.IsNullOrEmpty(brand.LogoUrl))
                {
                    DocumentSetting.Delete(brand.LogoUrl, "brands");
                }

                brand.LogoUrl = DocumentSetting.Upload(dto.LogoFile, "brands");
            }

            if (!string.IsNullOrEmpty(dto.Description)) brand.Description = dto.Description;

            _unitOfWork.Repository<ProductBrand, int>().Update(brand);
            await _unitOfWork.CompleteAsync();

            return new BrandOperationResponseDto
            {
                Success = true,
                Message = "Brand updated successfully",
                BrandId = id
            };
        }

        public async Task<BrandOperationResponseDto> DeleteBrandAsync(int id)
        {
            var brand = await _unitOfWork.Repository<ProductBrand, int>().GetAsync(id);
            if (brand == null)
                throw new KeyNotFoundException("Brand not found");

            // Check if brand has products
            var products = await _unitOfWork.Repository<Product, int>()
                .FindAsync(p => p.BrandId == id);

            if (products.Any())
                throw new InvalidOperationException($"Cannot delete brand. It has {products.Count()} product(s)");

            // Delete logo
            if (!string.IsNullOrEmpty(brand.LogoUrl))
            {
                DocumentSetting.Delete(brand.LogoUrl, "brands");
            }

            _unitOfWork.Repository<ProductBrand, int>().Delete(brand);
            await _unitOfWork.CompleteAsync();

            return new BrandOperationResponseDto
            {
                Success = true,
                Message = "Brand deleted successfully",
                BrandId = id
            };
        }

        // ==================== TYPE MANAGEMENT ====================

        public async Task<TypeOperationResponseDto> AddTypeAsync(AddProductTypeDto dto)
        {
            // Check if type name already exists
            var existing = await _unitOfWork.Repository<ProductType, int>()
                .FindAsync(t => t.Name.ToLower() == dto.Name.ToLower());

            if (existing.Any())
                throw new InvalidOperationException("Product type name already exists");

            var type = new ProductType
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ProductType, int>().AddAsync(type);
            await _unitOfWork.CompleteAsync();

            return new TypeOperationResponseDto
            {
                Success = true,
                Message = "Product type added successfully",
                TypeId = type.Id
            };
        }

        public async Task<TypeOperationResponseDto> UpdateTypeAsync(int id, UpdateProductTypeDto dto)
        {
            var type = await _unitOfWork.Repository<ProductType, int>().GetAsync(id);
            if (type == null)
                throw new KeyNotFoundException("Product type not found");

            // Check if new name conflicts with existing type
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name.ToLower() != type.Name.ToLower())
            {
                var existing = await _unitOfWork.Repository<ProductType, int>()
                    .FindAsync(t => t.Name.ToLower() == dto.Name.ToLower());

                if (existing.Any())
                    throw new InvalidOperationException("Product type name already exists");

                type.Name = dto.Name;
            }

            if (!string.IsNullOrEmpty(dto.Description)) type.Description = dto.Description;

            _unitOfWork.Repository<ProductType, int>().Update(type);
            await _unitOfWork.CompleteAsync();

            return new TypeOperationResponseDto
            {
                Success = true,
                Message = "Product type updated successfully",
                TypeId = id
            };
        }

        public async Task<TypeOperationResponseDto> DeleteTypeAsync(int id)
        {
            var type = await _unitOfWork.Repository<ProductType, int>().GetAsync(id);
            if (type == null)
                throw new KeyNotFoundException("Product type not found");

            // Check if type has products
            var products = await _unitOfWork.Repository<Product, int>()
                .FindAsync(p => p.TypeId == id);

            if (products.Any())
                throw new InvalidOperationException($"Cannot delete product type. It has {products.Count()} product(s)");

            _unitOfWork.Repository<ProductType, int>().Delete(type);
            await _unitOfWork.CompleteAsync();

            return new TypeOperationResponseDto
            {
                Success = true,
                Message = "Product type deleted successfully",
                TypeId = id
            };
        }
    }
}
