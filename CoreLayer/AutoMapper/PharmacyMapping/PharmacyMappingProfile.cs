using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Helper.Documents;
using Microsoft.Extensions.Configuration;

namespace CoreLayer.AutoMapper.PharmacyMapping
{
    public class PharmacyMappingProfile : Profile
    {
        public PharmacyMappingProfile(IConfiguration configuration)
        {
            var baseUrl = configuration["BaseURL"];

            // PharmacyProfile mappings
            CreateMap<PharmacyProfile, PharmacyProfileResponseDto>();

            // PharmacyApply mappings
            CreateMap<PharmacyApply, PharmacyApplicationDetailDto>();
            CreateMap<PharmacyApply, PharmacyApplicationSummaryDto>();

            // PharmacyListing mappings - In-memory mapping (not for ProjectTo)
            CreateMap<PharmacyListing, PharmacyListingResponseDto>()
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ImageUrls)
                        ? new List<string>()
                        : src.ImageUrls
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(img => DocumentSetting.GetFileUrl(img.Trim(), "pharmacy-listings", baseUrl))
                            .ToList()))
                .ForMember(dest => dest.PharmacyName, opt => opt.MapFrom(src =>
                    src.Pharmacy != null
                        ? $"{src.Pharmacy.FirstName} {src.Pharmacy.LastName}"
                        : string.Empty))
                .ForMember(dest => dest.SpeciesName, opt => opt.MapFrom(src =>
                    src.Species != null ? src.Species.Name : string.Empty));
        }
    }
}

/*
CreateMap<Accessory, AccessoryDto>()
    .ForMember(dest => dest.ImageUrl,
               opt => opt.MapFrom(src => $"{baseUrl}/{src.ImageUrl}"));

This won't work inside ProjectTo() because EF Core won't understand how to create a String in SQL using $"
so we should use normal mapping for such cases like we do in pharmacy service in GetMyListingsAsync.
 */