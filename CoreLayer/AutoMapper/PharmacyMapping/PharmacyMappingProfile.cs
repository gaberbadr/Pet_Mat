using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Helper.Documents;
using Microsoft.Extensions.Configuration;

namespace CoreLayer.AutoMapper.PharmacyMapping
{
    public class PharmacyMappingProfile : Profile
    {
        private readonly IConfiguration _configuration;

        public PharmacyMappingProfile(IConfiguration configuration)
        {
            _configuration = configuration;

            // PharmacyProfile mappings
            CreateMap<PharmacyProfile, PharmacyProfileResponseDto>();

            // PharmacyApply mappings
            CreateMap<PharmacyApply, PharmacyApplicationDetailDto>();
            CreateMap<PharmacyApply, PharmacyApplicationSummaryDto>();

            // PharmacyListing mappings
            CreateMap<PharmacyListing, PharmacyListingResponseDto>()
            .AfterMap((src, dest) =>
            {
                dest.ImageUrls = string.IsNullOrEmpty(src.ImageUrls)
                    ? new List<string>()
                    : src.ImageUrls
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(img => DocumentSetting.GetFileUrl(img.Trim(), "pharmacy-listings", _configuration["BaseURL"]))
                        .ToList();

                dest.PharmacyName = src.Pharmacy != null
                    ? $"{src.Pharmacy.FirstName} {src.Pharmacy.LastName}"
                    : string.Empty;

                dest.SpeciesName = src.Species?.Name;
            });

        }
    }
}