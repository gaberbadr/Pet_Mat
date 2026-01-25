using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Helper.Pagination;

namespace CoreLayer.Service_Interface.User
{
    public interface IUserDoctorManagement
    { 
        // ==================== Doctor INFO ====================
        Task<PaginationResponse<PublicDoctorProfileDto>> GetDoctorsAsync(DoctorFilterParams filterParams);
        Task<DoctorApplicationOperationResponseDto> ApplyToBeDoctorAsync(ApplyDoctorDto dto, string userId);
        Task<UserDoctorApplicationStatusDto> GetDoctorApplicationStatusAsync(string userId);
        Task<RatingOperationResponseDto> RateDoctorAsync(string doctorId, RateDoctorDto dto, string userId);
        Task<RatingOperationResponseDto> UpdateDoctorRatingAsync(string doctorId, RateDoctorDto dto, string userId);
        Task<PublicDoctorProfileDto> GetPublicDoctorProfileAsync(string doctorId);
        Task<DoctorRatingListDto> GetDoctorAllRatingsAsync(string doctorId);
    }
}
