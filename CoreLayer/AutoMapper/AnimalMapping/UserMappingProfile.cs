using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer.Dtos.Animals;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.Documents;
using Microsoft.Extensions.Configuration;

namespace CoreLayer.AutoMapper.AnimalMapping
{
    public class UserMappingProfile : Profile
    {
        private readonly IConfiguration _configuration;

        public UserMappingProfile(IConfiguration configuration)
        {
            _configuration = configuration;
            var baseUrl = _configuration["BaseURL"];

            // Animal mappings
            CreateMap<Animal, AnimalDto>()
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size.ToString()))
                .ForMember(dest => dest.SubSpeciesName,
                    opt => opt.MapFrom(src => src.SubSpecies != null ? src.SubSpecies.Name : null))
                .ForMember(dest => dest.ColorName,
                    opt => opt.MapFrom(src => src.Color != null ? src.Color.Name : null))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ImageUrl)
                        ? new List<string>()
                        : src.ImageUrl
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(img => DocumentSetting.GetFileUrl(img.Trim(), "animals", baseUrl))
                            .ToList()));

            CreateMap<Animal, AnimalResponseDto>()
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size.ToString()))
                .ForMember(dest => dest.SubSpeciesName,
                    opt => opt.MapFrom(src => src.SubSpecies != null ? src.SubSpecies.Name : null))
                .ForMember(dest => dest.ColorName,
                    opt => opt.MapFrom(src => src.Color != null ? src.Color.Name : null))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ImageUrl)
                        ? new List<string>()
                        : src.ImageUrl
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(img => DocumentSetting.GetFileUrl(img.Trim(), "animals", baseUrl))
                            .ToList()))
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
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(src.ProfilePicture, "profiles", baseUrl)
                        : null))
                .ForMember(dest => dest.City,
                    opt => opt.MapFrom(src => src.Address != null ? src.Address.City : null))
                .ForMember(dest => dest.Government,
                    opt => opt.MapFrom(src => src.Address != null ? src.Address.Government : null));

            // AnimalListing mappings
            CreateMap<AnimalListing, AnimalListingResponseDto>()
                .ForMember(dest => dest.Animal, opt => opt.MapFrom(src => src.Animal))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owner));
        }
    }
}