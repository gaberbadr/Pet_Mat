using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Doctor;

namespace CoreLayer.Service_Interface.Admin
{
    public interface IAdminDoctorApplicationManagement
    {
        // Doctor Application Management
        Task<DoctorApplicationListDto> GetPendingDoctorApplicationsAsync();
        Task<DoctorApplicationListDto> GetAllDoctorApplicationsAsync(string? status = null);
        Task<DoctorApplicationDetailDto> GetDoctorApplicationByIdAsync(Guid id);
        Task<ApplicationReviewResponseDto> ReviewDoctorApplicationAsync(Guid id, ReviewDoctorApplicationDto dto);
    }
}
