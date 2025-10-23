using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Admin;
using CoreLayer.Entities.Identity;
using CoreLayer.Service_Interface.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Admin
{
    public class AdminUserManagement : IAdminUserManagement
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AdminUserManagement(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _configuration = configuration;
        }

        // ==================== USER MANAGEMENT ====================

        public async Task<UserBlockResponseDto> BlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
                throw new InvalidOperationException("Cannot block users with Admin role");

            if (!user.IsActive)
                throw new InvalidOperationException("User is already blocked");

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to block user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            // Invalidate all active tokens immediately
            await _userManager.UpdateSecurityStampAsync(user);

            return new UserBlockResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                IsActive = user.IsActive,
                Message = "User blocked successfully"
            };
        }

        public async Task<UserBlockResponseDto> UnblockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
                throw new InvalidOperationException("Cannot unblock users with Admin role");

            if (user.IsActive)
                throw new InvalidOperationException("User is already active");

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to unblock user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return new UserBlockResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                IsActive = user.IsActive,
                Message = "User unblocked successfully"
            };
        }

    }
}
