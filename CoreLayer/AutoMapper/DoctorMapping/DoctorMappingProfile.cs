using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Entities.Doctors;

namespace CoreLayer.AutoMapper.DoctorMapping
{
    public class DoctorMappingProfile : Profile
    {
        public DoctorMappingProfile()
        {
            // DoctorProfile mappings
            CreateMap<DoctorProfile, DoctorProfileResponseDto>();

            // DoctorApply mappings
            CreateMap<DoctorApply, DoctorApplicationDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<DoctorApply, DoctorApplicationSummaryDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
