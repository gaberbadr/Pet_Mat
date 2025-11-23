using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer.Dtos.Community;
using CoreLayer.Entities.Community;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.Documents;
using Microsoft.Extensions.Configuration;

namespace CoreLayer.AutoMapper.CommunityMapping
{
    public class CommunityMappingProfile : Profile
    {
        private readonly string _baseUrl;

        public CommunityMappingProfile(IConfiguration configuration)
        {
            _baseUrl = configuration["BaseURL"];
            CreateMaps();
        }

        private void CreateMaps()
        {
            // ==================== POST MAPPINGS ====================

            CreateMap<Post, PostResponseDto>()
                .ForMember(dest => dest.AuthorId,
                    opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.AuthorName,
                    opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.AuthorProfilePicture,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.User.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(src.User.ProfilePicture, "profiles", _baseUrl)
                        : null))
                .ForMember(dest => dest.SpeciesName,
                    opt => opt.MapFrom(src => src.Species != null ? src.Species.Name : null))
                .ForMember(dest => dest.ImageUrls,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ImageUrls)
                        ? src.ImageUrls.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(img => DocumentSetting.GetFileUrl(img.Trim(), "posts", _baseUrl))
                            .ToList()
                        : new List<string>()))
                .ForMember(dest => dest.CommentsCount,
                    opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Count : 0))//get comments  Navigation Properties count 
                .ForMember(dest => dest.ReactionsCount,
                    opt => opt.MapFrom(src => src.Reactions != null ? src.Reactions.Count : 0))//get reactions Navigation Properties count
                .ForMember(dest => dest.ReactionsSummary,
                    opt => opt.MapFrom(src => src.Reactions != null
                        ? src.Reactions.GroupBy(r => r.Type.ToString())
                            .ToDictionary(g => g.Key, g => g.Count())
                        : new Dictionary<string, int>()))//Group reactions by type and count and convert to dictionary
                .ForMember(dest => dest.CurrentUserReaction,
                    opt => opt.Ignore()); // Set in service

            // ==================== COMMENT MAPPINGS ====================

            CreateMap<Comment, CommentResponseDto>()
                .ForMember(dest => dest.AuthorId,
                    opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.AuthorName,
                    opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.AuthorProfilePicture,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.User.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(src.User.ProfilePicture, "profiles", _baseUrl)
                        : null))
                .ForMember(dest => dest.Replies,
                    opt => opt.MapFrom(src => src.Replies != null ? src.Replies : new List<Comment>()));

            // ==================== REACTION MAPPINGS ====================

            CreateMap<PostReaction, ReactionResponseDto>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.UserProfilePicture,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.User.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(src.User.ProfilePicture, "profiles", _baseUrl)
                        : null))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type));
        }
    }

}
