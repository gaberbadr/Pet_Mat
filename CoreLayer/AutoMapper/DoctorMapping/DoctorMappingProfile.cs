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
            CreateMap<DoctorApply, DoctorApplicationDetailDto>();

            CreateMap<DoctorApply, DoctorApplicationSummaryDto>();
        }
    }
}
