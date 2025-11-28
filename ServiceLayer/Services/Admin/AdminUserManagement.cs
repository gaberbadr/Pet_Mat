using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Admin;
using CoreLayer.Entities.Doctors;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Helper.Documents;
using CoreLayer.Service_Interface.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using static CoreLayer.Dtos.Admin.AdminUsersManagementDTOs;

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

        // ==================== ROLE MANAGEMENT ====================

        public async Task<RoleOperationResponseDto> AddAdminAssistantRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);

            // Check if user already has any role
            if (userRoles.Any())
                throw new InvalidOperationException($"User already has role(s): {string.Join(", ", userRoles)}. Cannot add AdminAssistant role.");

            // Add AdminAssistant role
            var result = await _userManager.AddToRoleAsync(user, "AdminAssistant");

            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to add AdminAssistant role: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return new RoleOperationResponseDto
            {
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = "AdminAssistant",
                Message = "AdminAssistant role added successfully"
            };
        }

        public async Task<RoleOperationResponseDto> RemoveDoctorRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var isDoctor = await _userManager.IsInRoleAsync(user, "Doctor");
            if (!isDoctor)
                throw new InvalidOperationException("User does not have Doctor role");

            // Delete doctor profile
            var profile = (await _unitOfWork.Repository<DoctorProfile, Guid>()
                .FindAsync(dp => dp.UserId == userId)).FirstOrDefault();

            if (profile != null)
            {
                _unitOfWork.Repository<DoctorProfile, Guid>().Delete(profile);
            }

            // Delete doctor application
            var application = (await _unitOfWork.Repository<DoctorApply, Guid>()
                .FindAsync(da => da.UserId == userId)).FirstOrDefault();

            if (application != null)
            {
                // Delete uploaded documents
                if (!string.IsNullOrEmpty(application.NationalIdFront))
                    DocumentSetting.Delete(application.NationalIdFront, "doctor-documents");
                if (!string.IsNullOrEmpty(application.NationalIdBack))
                    DocumentSetting.Delete(application.NationalIdBack, "doctor-documents");
                if (!string.IsNullOrEmpty(application.SelfieWithId))
                    DocumentSetting.Delete(application.SelfieWithId, "doctor-documents");
                if (!string.IsNullOrEmpty(application.SyndicateCard))
                    DocumentSetting.Delete(application.SyndicateCard, "doctor-documents");
                if (!string.IsNullOrEmpty(application.MedicalLicense))
                    DocumentSetting.Delete(application.MedicalLicense, "doctor-documents");

                _unitOfWork.Repository<DoctorApply, Guid>().Delete(application);
            }

            // Delete doctor ratings
            await _unitOfWork.Repository<DoctorRating, int>()
                .DeleteRangeAsync(dr => dr.DoctorId == userId);

            // Remove Doctor role
            var result = await _userManager.RemoveFromRoleAsync(user, "Doctor");

            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to remove Doctor role: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _unitOfWork.CompleteAsync();

            return new RoleOperationResponseDto
            {
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = "Doctor",
                Message = "Doctor role and all related data removed successfully"
            };
        }

        public async Task<RoleOperationResponseDto> RemovePharmacyRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var isPharmacy = await _userManager.IsInRoleAsync(user, "Pharmacy");
            if (!isPharmacy)
                throw new InvalidOperationException("User does not have Pharmacy role");

            // Delete pharmacy profile
            var profile = (await _unitOfWork.Repository<PharmacyProfile, Guid>()
                .FindAsync(pp => pp.UserId == userId)).FirstOrDefault();

            if (profile != null)
            {
                _unitOfWork.Repository<PharmacyProfile, Guid>().Delete(profile);
            }

            // Soft delete all pharmacy listings
            var listings = await _unitOfWork.Repository<PharmacyListing, int>()
                .FindAsync(pl => pl.PharmacyId == userId && pl.IsActive);

            foreach (var listing in listings)
            {
                // Delete images
                if (!string.IsNullOrEmpty(listing.ImageUrls))
                {
                    var imageNames = listing.ImageUrls.Split(',');
                    foreach (var imageName in imageNames)
                    {
                        DocumentSetting.Delete(imageName.Trim(), "pharmacy-listings");
                    }
                }

                listing.IsActive = false;
                _unitOfWork.Repository<PharmacyListing, int>().Update(listing);
            }

            // Delete pharmacy application
            var application = (await _unitOfWork.Repository<PharmacyApply, Guid>()
                .FindAsync(pa => pa.UserId == userId)).FirstOrDefault();

            if (application != null)
            {
                // Delete uploaded documents
                if (!string.IsNullOrEmpty(application.PharmacyLicenseDocument))
                    DocumentSetting.Delete(application.PharmacyLicenseDocument, "pharmacy-documents");
                if (!string.IsNullOrEmpty(application.OwnerNationalIdFront))
                    DocumentSetting.Delete(application.OwnerNationalIdFront, "pharmacy-documents");
                if (!string.IsNullOrEmpty(application.OwnerNationalIdBack))
                    DocumentSetting.Delete(application.OwnerNationalIdBack, "pharmacy-documents");
                if (!string.IsNullOrEmpty(application.SelfieWithId))
                    DocumentSetting.Delete(application.SelfieWithId, "pharmacy-documents");
                if (!string.IsNullOrEmpty(application.SyndicateCard))
                    DocumentSetting.Delete(application.SyndicateCard, "pharmacy-documents");

                _unitOfWork.Repository<PharmacyApply, Guid>().Delete(application);
            }

            // Delete pharmacy ratings
            await _unitOfWork.Repository<PharmacyRating, int>()
                .DeleteRangeAsync(pr => pr.PharmacyId == userId);

            // Remove Pharmacy role
            var result = await _userManager.RemoveFromRoleAsync(user, "Pharmacy");

            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to remove Pharmacy role: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _unitOfWork.CompleteAsync();

            return new RoleOperationResponseDto
            {
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = "Pharmacy",
                Message = "Pharmacy role and all related data removed successfully"
            };
        }

        public async Task<RoleOperationResponseDto> RemoveAdminAssistantRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var isAdminAssistant = await _userManager.IsInRoleAsync(user, "AdminAssistant");
            if (!isAdminAssistant)
                throw new InvalidOperationException("User does not have AdminAssistant role");

            // Remove AdminAssistant role
            var result = await _userManager.RemoveFromRoleAsync(user, "AdminAssistant");

            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to remove AdminAssistant role: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return new RoleOperationResponseDto
            {
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = "AdminAssistant",
                Message = "AdminAssistant role removed successfully"
            };
        }
    }
}
