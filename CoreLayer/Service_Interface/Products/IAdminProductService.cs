using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Products;

namespace CoreLayer.Service_Interface.Products
{
    public interface IAdminProductService
    {
        // Product management
        Task<ProductOperationResponseDto> AddProductAsync(AddProductDto dto);
        Task<ProductOperationResponseDto> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<ProductOperationResponseDto> DeleteProductAsync(int id);
        Task<ProductOperationResponseDto> ActivateProductAsync(int id);
        Task<ProductOperationResponseDto> DeactivateProductAsync(int id);

        // Brand management
        Task<BrandOperationResponseDto> AddBrandAsync(AddProductBrandDto dto);
        Task<BrandOperationResponseDto> UpdateBrandAsync(int id, UpdateProductBrandDto dto);
        Task<BrandOperationResponseDto> DeleteBrandAsync(int id);

        // Type management
        Task<TypeOperationResponseDto> AddTypeAsync(AddProductTypeDto dto);
        Task<TypeOperationResponseDto> UpdateTypeAsync(int id, UpdateProductTypeDto dto);
        Task<TypeOperationResponseDto> DeleteTypeAsync(int id);
    }
}
