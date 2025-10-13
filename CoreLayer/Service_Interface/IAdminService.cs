using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Admin;
using CoreLayer.Dtos.Doctor;

namespace CoreLayer.Service_Interface
{
    public interface IAdminService
    {
        // User Management
        Task<UserBlockResponseDto> BlockUserAsync(string userId);
        Task<UserBlockResponseDto> UnblockUserAsync(string userId);

        // Species Management
        Task<SpeciesResponseDto> AddSpeciesAsync(SpeciesAdminDto dto);
        Task<DeleteResponseDto> DeleteSpeciesAsync(int id);

        // SubSpecies Management
        Task<SubSpeciesResponseDto> AddSubSpeciesAsync(SubSpeciesAdminDto dto);
        Task<DeleteResponseDto> DeleteSubSpeciesAsync(int id);

        // Color Management
        Task<ColorResponseDto> AddColorAsync(ColorAdminDto dto);
        Task<DeleteResponseDto> DeleteColorAsync(int id);

        // Doctor Application Management
        Task<DoctorApplicationListDto> GetPendingDoctorApplicationsAsync();
        Task<DoctorApplicationListDto> GetAllDoctorApplicationsAsync(string? status = null);
        Task<DoctorApplicationDetailDto> GetDoctorApplicationByIdAsync(Guid id);
        Task<ApplicationReviewResponseDto> ReviewDoctorApplicationAsync(Guid id, ReviewDoctorApplicationDto dto);
    }
}
