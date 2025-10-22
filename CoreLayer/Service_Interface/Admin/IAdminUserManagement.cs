using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Admin;

namespace CoreLayer.Service_Interface.Admin
{
    public interface IAdminUserManagement
    {
        // User Management
        Task<UserBlockResponseDto> BlockUserAsync(string userId);
        Task<UserBlockResponseDto> UnblockUserAsync(string userId);

    }
}
