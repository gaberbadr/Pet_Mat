using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Dtos.Admin
{
    public class AdminUsersManagementDTOs
    {
        // ==================== ROLE OPERATION DTOs ====================

        public class RoleOperationResponseDto
        {
            public bool Success { get; set; }
            public string UserId { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; }
            public string Message { get; set; }
        }

        // ==================== USER BLOCK DTOs ====================

        public class UserBlockResponseDto
        {
            public string UserId { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public bool IsActive { get; set; }
            public string Message { get; set; }
        }
    }
}
