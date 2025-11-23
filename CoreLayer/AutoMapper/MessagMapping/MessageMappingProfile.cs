using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer.Dtos.Messag;
using CoreLayer.Dtos.Notification;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Messages;
using CoreLayer.Entities.Notifications;
using CoreLayer.Helper.Documents;
using Microsoft.Extensions.Configuration;

namespace CoreLayer.AutoMapper.MessagMapping
{
    public class MessageMappingProfile : Profile
    {
        private readonly string _baseUrl;

        public MessageMappingProfile(IConfiguration configuration)
        {
            _baseUrl = configuration["BaseURL"];

            CreateMaps();
        }

        private void CreateMaps()
        {
            // ==================== MESSAGE MAPPINGS ====================

            CreateMap<Message, MessageResponseDto>()
                .ForMember(dest => dest.SenderName,
                    opt => opt.MapFrom(src => $"{src.Sender.FirstName} {src.Sender.LastName}"))
                .ForMember(dest => dest.SenderProfilePicture,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Sender.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(src.Sender.ProfilePicture, "profiles", _baseUrl)
                        : null))
                .ForMember(dest => dest.ReceiverName,
                    opt => opt.MapFrom(src => $"{src.Receiver.FirstName} {src.Receiver.LastName}"))
                .ForMember(dest => dest.ReceiverProfilePicture,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Receiver.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(src.Receiver.ProfilePicture, "profiles", _baseUrl)
                        : null))
                .ForMember(dest => dest.ContextInfo,
                    opt => opt.Ignore()); // This will be set separately by service

            // ==================== CONVERSATION MAPPINGS ====================

            // This is handled in the service layer due to grouping logic
            // But we can create a helper mapping for user info
            CreateMap<ApplicationUser, ConversationUserInfoDto>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.UserProfilePicture,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(src.ProfilePicture, "profiles", _baseUrl)
                        : null));

            // ==================== BLOCKED USER MAPPINGS ====================

            CreateMap<UserBlock, BlockedUserDto>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.BlockedId))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => $"{src.Blocked.FirstName} {src.Blocked.LastName}"))
                .ForMember(dest => dest.UserProfilePicture,
                    opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Blocked.ProfilePicture)
                        ? DocumentSetting.GetFileUrl(src.Blocked.ProfilePicture, "profiles", _baseUrl)
                        : null))
                .ForMember(dest => dest.BlockedAt,
                    opt => opt.MapFrom(src => src.CreatedAt));

        }
    }
}
