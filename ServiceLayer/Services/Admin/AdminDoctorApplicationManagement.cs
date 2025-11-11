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
using CoreLayer.Enums;
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
                .FindAsync(da => da.Status == ApplicationStatus.Pending);

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

        public async Task<DoctorApplicationListDto> GetAllDoctorApplicationsAsync(ApplicationStatus? status = null)
        {
            var repo = _unitOfWork.Repository<DoctorApply, Guid>();
            var applications = status == null
                ? await repo.GetAllAsync()
                : await repo.FindAsync(da => da.Status == status);

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
            var application = await _unitOfWork.Repository<DoctorApply, Guid>().GetAsync(id)
                ?? throw new KeyNotFoundException("Application not found");

            if (application.Status != ApplicationStatus.Pending)
                throw new InvalidOperationException($"Application already {application.Status}");

            if (dto.Status != ApplicationStatus.Approved && dto.Status != ApplicationStatus.Rejected)
                throw new InvalidOperationException("Status must be either Approved or Rejected");

            var user = await _userManager.FindByIdAsync(application.UserId)
                ?? throw new KeyNotFoundException("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                _unitOfWork.Repository<DoctorApply, Guid>().Delete(application);
                await _unitOfWork.CompleteAsync();
                throw new InvalidOperationException("User already has a role and cannot apply for the service.");
            }

            // Update application
            application.Status = dto.Status;
            application.RejectionReason = dto.RejectionReason;
            _unitOfWork.Repository<DoctorApply, Guid>().Update(application);

            if (dto.Status == ApplicationStatus.Approved)
            {
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

                if (!await _userManager.IsInRoleAsync(user, "Doctor"))
                {
                    await _userManager.AddToRoleAsync(user, "Doctor");
                }
            }

            await _unitOfWork.CompleteAsync();

            return new ApplicationReviewResponseDto
            {
                Success = true,
                Message = dto.Status == ApplicationStatus.Approved
                    ? "Application approved and doctor profile created successfully"
                    : "Application rejected",
                Status = dto.Status.ToString()
            }; 
        }

    }
}
