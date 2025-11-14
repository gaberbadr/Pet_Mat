using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Products;
using CoreLayer.Entities.Foods;

namespace CoreLayer.Specifications.Products
{
    public class ProductsSpecification
    {
        public class ProductFilterSpecification : BaseSpecifications<Product, int>
        {
            public ProductFilterSpecification(ProductFilterParams filterParams)
                : base(BuildCriteria(filterParams))
            {
                Includes.Add(p => p.Brand);
                Includes.Add(p => p.Type);
                Includes.Add(p => p.Species);

                // Apply sorting
                switch (filterParams.SortBy?.ToLower())
                {
                    case "price_asc":
                        OrderBy = p => p.Price;
                        break;
                    case "price_desc":
                        OrderByDescending = p => p.Price;
                        break;
                    case "name":
                        OrderBy = p => p.Name;
                        break;
                    case "newest":
                    default:
                        OrderByDescending = p => p.CreatedAt;
                        break;
                }

                applyPagnation(
                    (filterParams.PageIndex - 1) * filterParams.PageSize,
                    filterParams.PageSize
                );
            }

            private static Expression<Func<Product, bool>> BuildCriteria(ProductFilterParams filterParams)
            {
                return p =>
                    p.IsActive == true &&
                    (!filterParams.BrandId.HasValue || p.BrandId == filterParams.BrandId.Value) &&
                    (!filterParams.TypeId.HasValue || p.TypeId == filterParams.TypeId.Value) &&
                    (!filterParams.SpeciesId.HasValue || p.SpeciesId == filterParams.SpeciesId.Value) &&
                    (!filterParams.MinPrice.HasValue || p.Price >= filterParams.MinPrice.Value) &&
                    (!filterParams.MaxPrice.HasValue || p.Price <= filterParams.MaxPrice.Value) &&
                    (!filterParams.InStock.HasValue || (filterParams.InStock.Value ? p.Stock > 0 : p.Stock == 0)) &&
                    (string.IsNullOrEmpty(filterParams.Search) ||
                        p.Name.ToLower().Contains(filterParams.Search.ToLower()) ||
                        p.Description.ToLower().Contains(filterParams.Search.ToLower()));
            }
        }

        public class ProductCountSpecification : BaseSpecifications<Product, int>
        {
            public ProductCountSpecification(ProductFilterParams filterParams)
                : base(BuildCriteria(filterParams))
            {
            }

            private static Expression<Func<Product, bool>> BuildCriteria(ProductFilterParams filterParams)
            {
                return p =>
                    p.IsActive == true &&
                    (!filterParams.BrandId.HasValue || p.BrandId == filterParams.BrandId.Value) &&
                    (!filterParams.TypeId.HasValue || p.TypeId == filterParams.TypeId.Value) &&
                    (!filterParams.SpeciesId.HasValue || p.SpeciesId == filterParams.SpeciesId.Value) &&
                    (!filterParams.MinPrice.HasValue || p.Price >= filterParams.MinPrice.Value) &&
                    (!filterParams.MaxPrice.HasValue || p.Price <= filterParams.MaxPrice.Value) &&
                    (!filterParams.InStock.HasValue || (filterParams.InStock.Value ? p.Stock > 0 : p.Stock == 0)) &&
                    (string.IsNullOrEmpty(filterParams.Search) ||
                        p.Name.ToLower().Contains(filterParams.Search.ToLower()) ||
                        p.Description.ToLower().Contains(filterParams.Search.ToLower()));
            }
        }

        public class ProductByIdSpecification : BaseSpecifications<Product, int>
        {
            public ProductByIdSpecification(int id)
                : base(p => p.Id == id)
            {
                Includes.Add(p => p.Brand);
                Includes.Add(p => p.Type);
                Includes.Add(p => p.Species);
            }
        }
    }
}
