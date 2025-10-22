using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Dtos.Pharmacy;

namespace CoreLayer.Service_Interface.Admin
{
    public interface IAdminPharmacyApplicationManagement
    {
        Task<PharmacyApplicationListDto> GetPendingPharmacyApplicationsAsync();
        Task<PharmacyApplicationListDto> GetAllPharmacyApplicationsAsync(string? status = null);
        Task<PharmacyApplicationDetailDto> GetPharmacyApplicationByIdAsync(Guid id);
        Task<ApplicationReviewResponseDto> ReviewPharmacyApplicationAsync(Guid id, ReviewPharmacyApplicationDto dto);
    }
}
