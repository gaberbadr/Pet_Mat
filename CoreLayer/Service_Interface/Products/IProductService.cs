using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Products;
using CoreLayer.Helper.Pagination;

namespace CoreLayer.Service_Interface.Products
{
    public interface IProductService
    {
        // User endpoints
        Task<PaginationResponse<ProductDto>> GetAllProductsAsync(ProductFilterParams filterParams);
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductBrandListDto> GetAllBrandsAsync();
        Task<ProductTypeListDto> GetAllTypesAsync();
    }
}
