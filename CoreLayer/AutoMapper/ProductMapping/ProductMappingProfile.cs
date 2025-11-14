using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer.Dtos.Animals;
using CoreLayer.Dtos.Products;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Foods;

namespace CoreLayer.AutoMapper.ProductMapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<ProductType, ProductTypeDto>();
            CreateMap<Species, SpeciesDto>();
        }
    }
}
