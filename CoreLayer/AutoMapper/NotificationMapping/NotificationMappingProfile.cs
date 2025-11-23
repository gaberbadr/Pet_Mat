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

namespace CoreLayer.AutoMapper.NotificationMapping
{
    public class NotificationMappingProfile : Profile
    {

        public NotificationMappingProfile()
        {

            // ==================== NOTIFICATION MAPPINGS ====================

            CreateMap<Notification, NotificationDto>();
        }


    }
}
