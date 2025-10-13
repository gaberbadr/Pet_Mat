using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Doctor;

namespace CoreLayer.Service_Interface
{
    public interface IDoctorService
    {
        // Profile Management
        Task<DoctorProfileResponseDto> GetDoctorProfileAsync(string userId);
        Task<DoctorProfileOperationResponseDto> UpdateDoctorProfileAsync(string userId, UpdateDoctorProfileDto dto);
        Task<DoctorProfileOperationResponseDto> UpdateDoctorLocationAsync(string userId, UpdateDoctorLocationDto dto);
        Task<DoctorProfileOperationResponseDto> DeleteDoctorAccountAsync(string userId);

        // Ratings
        Task<DoctorRatingListDto> GetDoctorRatingsAsync(string doctorId);
    }
}
