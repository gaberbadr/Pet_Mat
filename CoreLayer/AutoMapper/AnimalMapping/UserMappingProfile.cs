using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer.Dtos.User;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;

namespace CoreLayer.AutoMapper.AnimalMapping
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // Animal mappings
            CreateMap<Animal, AnimalDto>()
                .ForMember(dest => dest.SubSpeciesName,
                    opt => opt.MapFrom(src => src.SubSpecies != null ? src.SubSpecies.Name : null))
                .ForMember(dest => dest.ColorName,
                    opt => opt.MapFrom(src => src.Color != null ? src.Color.Name : null));

            CreateMap<Animal, AnimalResponseDto>()
                .ForMember(dest => dest.SubSpeciesName,
                    opt => opt.MapFrom(src => src.SubSpecies != null ? src.SubSpecies.Name : null))
                .ForMember(dest => dest.ColorName,
                    opt => opt.MapFrom(src => src.Color != null ? src.Color.Name : null))
                .ForMember(dest => dest.Message, opt => opt.Ignore());

            // Species mappings
            CreateMap<Species, SpeciesDto>();
            CreateMap<Species, SpeciesInfoDto>();

            // SubSpecies mappings
            CreateMap<SubSpecies, SubSpeciesInfoDto>();

            // Color mappings
            CreateMap<Color, ColorInfoDto>();

            // User/Owner mappings
            CreateMap<ApplicationUser, OwnerDto>()
                .ForMember(dest => dest.City,
                    opt => opt.MapFrom(src => src.Address != null ? src.Address.City : null))
                .ForMember(dest => dest.Government,
                    opt => opt.MapFrom(src => src.Address != null ? src.Address.Government : null));

            // AnimalListing mappings
            CreateMap<AnimalListing, AnimalListingResponseDto>();
        }
    }
}
