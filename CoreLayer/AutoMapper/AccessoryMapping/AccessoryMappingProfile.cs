using AutoMapper;
using CoreLayer.Dtos.Accessory;
using CoreLayer.Dtos.User;
using CoreLayer.Entities.Accessories;
using CoreLayer.Helper.Documents;
using Microsoft.Extensions.Configuration;

public class AccessoryMappingProfile : Profile
{
    private readonly IConfiguration _configuration;

    public AccessoryMappingProfile(IConfiguration configuration)
    {
        _configuration = configuration;
        var baseUrl = _configuration["BaseURL"];

        CreateMap<AccessoryListing, AccessoryListingResponseDto>()
            .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Condition.ToString()))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => new OwnerDto
            {
                Id = src.Owner.Id,
                FirstName = src.Owner.FirstName,
                LastName = src.Owner.LastName,
                ProfilePicture = !string.IsNullOrEmpty(src.Owner.ProfilePicture)
                    ? DocumentSetting.GetFileUrl(src.Owner.ProfilePicture, "users", baseUrl)
                    : null,
                City = src.Owner.Address != null ? src.Owner.Address.City : null,
                Government = src.Owner.Address != null ? src.Owner.Address.Government : null
            }))
            .ForMember(dest => dest.Species, opt => opt.MapFrom(src =>
                src.Species != null
                    ? new SpeciesDto { Id = src.Species.Id, Name = src.Species.Name }
                    : null))
            // FIX: Map ImageUrls directly instead of using AfterMap
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.ImageUrls)
                    ? new List<string>()
                    : src.ImageUrls
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(img => DocumentSetting.GetFileUrl(img.Trim(), "accessory-listings", baseUrl))
                        .ToList()));
    }
}

/*
CreateMap<Accessory, AccessoryDto>()
    .ForMember(dest => dest.ImageUrl,
               opt => opt.MapFrom(src => $"{baseUrl}/{src.ImageUrl}"));

This won't work inside ProjectTo() because EF Core won't understand how to create a String in SQL using $"
so we should use normal mapping for such cases like we do in accessory service in GetMyAccessoryListingsAsync.
 */