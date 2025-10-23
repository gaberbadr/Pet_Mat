using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Entities.Doctors;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.Documents;
using CoreLayer.Service_Interface.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Admin
{
    public class AdminDoctorApplicationManagement : IAdminDoctorApplicationManagement
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AdminDoctorApplicationManagement(
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


        // ==================== DOCTOR APPLICATION MANAGEMENT ====================

        public async Task<DoctorApplicationListDto> GetPendingDoctorApplicationsAsync()
        {
            var applications = await _unitOfWork.Repository<DoctorApply, Guid>()
                .FindAsync(da => da.Status == "Pending");

            var applicationDtos = new List<DoctorApplicationSummaryDto>();

            foreach (var app in applications)
            {
                var user = await _userManager.FindByIdAsync(app.UserId);
                if (user != null)
                {
                    applicationDtos.Add(new DoctorApplicationSummaryDto
                    {
                        Id = app.Id,
                        UserId = app.UserId,
                        UserEmail = user.Email,
                        UserFullName = $"{user.FirstName} {user.LastName}",
                        Specialization = app.Specialization,
                        ExperienceYears = app.ExperienceYears,
                        Status = app.Status,
                        AppliedAt = app.AppliedAt
                    });
                }
            }

            return new DoctorApplicationListDto
            {
                Count = applicationDtos.Count,
                Data = applicationDtos.OrderByDescending(a => a.AppliedAt)
            };
        }

        public async Task<DoctorApplicationListDto> GetAllDoctorApplicationsAsync(string? status = null)
        {
            var applications = string.IsNullOrEmpty(status)
                ? await _unitOfWork.Repository<DoctorApply, Guid>().GetAllAsync()
                : await _unitOfWork.Repository<DoctorApply, Guid>().FindAsync(da => da.Status == status);

            var applicationDtos = new List<DoctorApplicationSummaryDto>();

            foreach (var app in applications)
            {
                var user = await _userManager.FindByIdAsync(app.UserId);
                if (user != null)
                {
                    applicationDtos.Add(new DoctorApplicationSummaryDto
                    {
                        Id = app.Id,
                        UserId = app.UserId,
                        UserEmail = user.Email,
                        UserFullName = $"{user.FirstName} {user.LastName}",
                        Specialization = app.Specialization,
                        ExperienceYears = app.ExperienceYears,
                        Status = app.Status,
                        AppliedAt = app.AppliedAt
                    });
                }
            }

            return new DoctorApplicationListDto
            {
                Count = applicationDtos.Count,
                Data = applicationDtos.OrderByDescending(a => a.AppliedAt)
            };
        }

        public async Task<DoctorApplicationDetailDto> GetDoctorApplicationByIdAsync(Guid id)
        {
            var application = await _unitOfWork.Repository<DoctorApply, Guid>().GetAsync(id);
            if (application == null)
                throw new KeyNotFoundException("Application not found");

            var user = await _userManager.FindByIdAsync(application.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var baseUrl = _configuration["BaseURL"];

            return new DoctorApplicationDetailDto
            {
                Id = application.Id,
                UserId = application.UserId,
                UserEmail = user.Email,
                UserFullName = $"{user.FirstName} {user.LastName}",
                Specialization = application.Specialization,
                ExperienceYears = application.ExperienceYears,
                ClinicAddress = application.ClinicAddress,
                NationalIdFront = DocumentSetting.GetFileUrl(application.NationalIdFront, "doctor-documents", baseUrl),
                NationalIdBack = DocumentSetting.GetFileUrl(application.NationalIdBack, "doctor-documents", baseUrl),
                SelfieWithId = DocumentSetting.GetFileUrl(application.SelfieWithId, "doctor-documents", baseUrl),
                SyndicateCard = DocumentSetting.GetFileUrl(application.SyndicateCard, "doctor-documents", baseUrl),
                MedicalLicense = DocumentSetting.GetFileUrl(application.MedicalLicense, "doctor-documents", baseUrl),
                Status = application.Status,
                AppliedAt = application.AppliedAt,
                RejectionReason = application.RejectionReason
            };
        }

        public async Task<ApplicationReviewResponseDto> ReviewDoctorApplicationAsync(Guid id, ReviewDoctorApplicationDto dto)
        {
            var application = await _unitOfWork.Repository<DoctorApply, Guid>().GetAsync(id);
            if (application == null)
                throw new KeyNotFoundException("Application not found");

            if (application.Status != "Pending")
                throw new InvalidOperationException($"Application already {application.Status.ToLower()}");

            // Validate status
            if (dto.Status != "Approved" && dto.Status != "Rejected")
                throw new InvalidOperationException("Status must be either 'Approved' or 'Rejected'");

            // Get user
            var user = await _userManager.FindByIdAsync(application.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            //Check if user already has any role
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                // Delete this application since the user already has a role
                _unitOfWork.Repository<DoctorApply, Guid>().Delete(application);
                await _unitOfWork.CompleteAsync();

                throw new InvalidOperationException("User already has a role and cannot apply for the service.");
            }

            // Update application
            application.Status = dto.Status;
            application.RejectionReason = dto.RejectionReason;

            _unitOfWork.Repository<DoctorApply, Guid>().Update(application);

            if (dto.Status == "Approved")
            {
                // Create doctor profile
                var profile = new DoctorProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = application.UserId,
                    Latitude = 0,
                    Longitude = 0,
                    Specialization = application.Specialization,
                    ExperienceYears = application.ExperienceYears,
                    ClinicAddress = application.ClinicAddress,
                    ClinicName = string.Empty,
                    Bio = string.Empty,
                    WorkingHours = "9 AM - 5 PM",
                    Phone = string.Empty,
                    IsAvailableForConsultation = false,
                    IsActive = true,
                    Services = string.Empty,
                    Languages = string.Empty,
                    AverageRating = 0,
                    TotalRatings = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<DoctorProfile, Guid>().AddAsync(profile);

                // Add Doctor role to user
                if (user != null)
                {
                    var roleExists = await _userManager.IsInRoleAsync(user, "Doctor");
                    if (!roleExists)
                    {
                        await _userManager.AddToRoleAsync(user, "Doctor");
                        // Invalidate all active tokens immediately
                        await _userManager.UpdateSecurityStampAsync(user);
                    }
                }
            }

            await _unitOfWork.CompleteAsync();

            return new ApplicationReviewResponseDto
            {
                Success = true,
                Message = dto.Status == "Approved"
                    ? "Application approved and doctor profile created successfully"
                    : "Application rejected",
                Status = dto.Status
            };
        }

    }
}
