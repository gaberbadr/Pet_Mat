using CoreLayer.Dtos.Products;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    public class ProductController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }


        /// Get all products with filters and pagination
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginationResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginationResponse<ProductDto>>> GetAllProducts([FromQuery] ProductFilterParams filterParams)
        {
            try
            {
                var result = await _productService.GetAllProductsAsync(filterParams);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }


        /// Get product by ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            try
            {
                var result = await _productService.GetProductByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }


        /// Get all product brands
        [HttpGet("brands")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductBrandListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProductBrandListDto>> GetAllBrands()
        {
            var result = await _productService.GetAllBrandsAsync();
            return Ok(result);
        }


        /// Get all product types
        [HttpGet("types")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductTypeListDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProductTypeListDto>> GetAllTypes()
        {
            var result = await _productService.GetAllTypesAsync();
            return Ok(result);
        }
    }


}
