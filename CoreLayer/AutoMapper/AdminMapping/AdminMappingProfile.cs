using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer.Dtos.Admin;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
using static CoreLayer.Dtos.Admin.AdminUsersManagementDTOs;

namespace CoreLayer.AutoMapper.AdminMapping
{
    public class AdminMappingProfile : Profile
    {
        public AdminMappingProfile()
        {
            // Species mappings
            CreateMap<Species, SpeciesResponseDto>()
                .ForMember(dest => dest.Message, opt => opt.Ignore());

            // SubSpecies mappings
            CreateMap<SubSpecies, SubSpeciesResponseDto>()
                .ForMember(dest => dest.SpeciesName,
                    opt => opt.MapFrom(src => src.Species != null ? src.Species.Name : string.Empty))
                .ForMember(dest => dest.Message, opt => opt.Ignore());

            // Color mappings
            CreateMap<Color, ColorResponseDto>()
                .ForMember(dest => dest.Message, opt => opt.Ignore());

            // User mappings
            CreateMap<ApplicationUser, UserBlockResponseDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Message, opt => opt.Ignore());
        }
    }
}
