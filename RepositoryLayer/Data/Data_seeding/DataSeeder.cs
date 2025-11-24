using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Orders;

namespace RepositoryLayer.Data.Data_seeding
{
    public static class DataSeeder
    {
        public static async Task SeedDeliveryMethodsAsync(IUnitOfWork unitOfWork)
        {
            if (!(await unitOfWork.Repository<DeliveryMethod, int>().GetAllAsync()).Any())
            {
                var methods = new List<DeliveryMethod>
                {
                    new DeliveryMethod
                    {
                        ShortName = "Standard",
                        Description = "Standard delivery",
                        DeliveryTime = "5-7 business days",
                        Cost = 5.00m,
                        CreatedAt = DateTime.UtcNow
                    },
                    new DeliveryMethod
                    {
                        ShortName = "Express",
                        Description = "Express delivery",
                        DeliveryTime = "2-3 business days",
                        Cost = 10.00m,
                        CreatedAt = DateTime.UtcNow
                    },
                    new DeliveryMethod
                    {
                        ShortName = "Next Day",
                        Description = "Next day delivery",
                        DeliveryTime = "1 business day",
                        Cost = 20.00m,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await unitOfWork.Repository<DeliveryMethod, int>().AddRangeAsync(methods);
                await unitOfWork.CompleteAsync();
            }
        }

        public static async Task SeedProductBrandsAsync(IUnitOfWork unitOfWork)
        {
            if (!(await unitOfWork.Repository<ProductBrand, int>().GetAllAsync()).Any())
            {
                var brands = new List<ProductBrand>
                {
                    new ProductBrand
                    {
                        Name = "Pedigree",
                        Description = "Quality pet food brand",
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductBrand
                    {
                        Name = "Royal Canin",
                        Description = "Premium pet nutrition",
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductBrand
                    {
                        Name = "Whiskas",
                        Description = "Cat food specialist",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await unitOfWork.Repository<ProductBrand, int>().AddRangeAsync(brands);
                await unitOfWork.CompleteAsync();
            }
        }

        public static async Task SeedProductTypesAsync(IUnitOfWork unitOfWork)
        {
            if (!(await unitOfWork.Repository<ProductType, int>().GetAllAsync()).Any())
            {
                var types = new List<ProductType>
                {
                    new ProductType
                    {
                        Name = "Dry Food",
                        Description = "Dry pet food",
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductType
                    {
                        Name = "Wet Food",
                        Description = "Canned pet food",
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductType
                    {
                        Name = "Treats",
                        Description = "Pet treats and snacks",
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductType
                    {
                        Name = "Supplements",
                        Description = "Health supplements",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await unitOfWork.Repository<ProductType, int>().AddRangeAsync(types);
                await unitOfWork.CompleteAsync();
            }
        }
    }
}
