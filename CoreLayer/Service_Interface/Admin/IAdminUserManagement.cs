using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Admin;
using static CoreLayer.Dtos.Admin.AdminUsersManagementDTOs;

namespace CoreLayer.Service_Interface.Admin
{
    public interface IAdminUserManagement
    {
        // ==================== USER BLOCKING ====================
        Task<UserBlockResponseDto> BlockUserAsync(string userId);
        Task<UserBlockResponseDto> UnblockUserAsync(string userId);

        // ==================== ROLE MANAGEMENT ====================
        Task<RoleOperationResponseDto> AddAdminAssistantRoleAsync(string userId);
        Task<RoleOperationResponseDto> RemoveDoctorRoleAsync(string userId);
        Task<RoleOperationResponseDto> RemovePharmacyRoleAsync(string userId);
        Task<RoleOperationResponseDto> RemoveAdminAssistantRoleAsync(string userId);

    }
}
