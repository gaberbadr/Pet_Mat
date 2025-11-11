using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Entities.Doctors;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Enums;
using CoreLayer.Helper.Documents;
using CoreLayer.Service_Interface.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Admin
{
    public class AdminPharmacyApplicationManagement : IAdminPharmacyApplicationManagement
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AdminPharmacyApplicationManagement(
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

        public async Task<PharmacyApplicationListDto> GetPendingPharmacyApplicationsAsync()
        {
            var applications = await _unitOfWork.Repository<PharmacyApply, Guid>()
                .FindAsync(pa => pa.Status == ApplicationStatus.Pending);

            var applicationDtos = new List<PharmacyApplicationSummaryDto>();

            foreach (var app in applications)
            {
                var user = await _userManager.FindByIdAsync(app.UserId);
                if (user != null)
                {
                    applicationDtos.Add(new PharmacyApplicationSummaryDto
                    {
                        Id = app.Id,
                        UserId = app.UserId,
                        UserEmail = user.Email,
                        UserFullName = $"{user.FirstName} {user.LastName}",
                        PharmacyName = app.PharmacyName,
                        Address = app.Address,
                        Phone = app.Phone,
                        LicenseNumber = app.LicenseNumber,
                        Status = app.Status.ToString(),
                        AppliedAt = app.AppliedAt
                    });
                }
            }

            return new PharmacyApplicationListDto
            {
                Count = applicationDtos.Count,
                Data = applicationDtos.OrderByDescending(a => a.AppliedAt)
            };
        }

        public async Task<PharmacyApplicationListDto> GetAllPharmacyApplicationsAsync(ApplicationStatus? status = null)
        {
            var repo = _unitOfWork.Repository<PharmacyApply, Guid>();
            var applications = status == null
                ? await repo.GetAllAsync()
                : await repo.FindAsync(da => da.Status == status);

            var applicationDtos = new List<PharmacyApplicationSummaryDto>();

            foreach (var app in applications)
            {
                var user = await _userManager.FindByIdAsync(app.UserId);
                if (user != null)
                {
                    applicationDtos.Add(new PharmacyApplicationSummaryDto
                    {
                        Id = app.Id,
                        UserId = app.UserId,
                        UserEmail = user.Email,
                        UserFullName = $"{user.FirstName} {user.LastName}",
                        PharmacyName = app.PharmacyName,
                        Address = app.Address,
                        Phone = app.Phone,
                        LicenseNumber = app.LicenseNumber,
                        Status = app.Status.ToString(),
                        AppliedAt = app.AppliedAt
                    });
                }
            }

            return new PharmacyApplicationListDto
            {
                Count = applicationDtos.Count,
                Data = applicationDtos.OrderByDescending(a => a.AppliedAt)
            };
        }

        public async Task<PharmacyApplicationDetailDto> GetPharmacyApplicationByIdAsync(Guid id)
        {
            var application = await _unitOfWork.Repository<PharmacyApply, Guid>().GetAsync(id);
            if (application == null)
                throw new KeyNotFoundException("Application not found");

            var user = await _userManager.FindByIdAsync(application.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var baseUrl = _configuration["BaseURL"];

            return new PharmacyApplicationDetailDto
            {
                Id = application.Id,
                UserId = application.UserId,
                UserEmail = user.Email,
                UserFullName = $"{user.FirstName} {user.LastName}",
                PharmacyName = application.PharmacyName,
                Address = application.Address,
                Phone = application.Phone,
                LicenseNumber = application.LicenseNumber,
                PharmacyLicenseDocument = DocumentSetting.GetFileUrl(application.PharmacyLicenseDocument, "pharmacy-documents", baseUrl),
                OwnerNationalIdFront = DocumentSetting.GetFileUrl(application.OwnerNationalIdFront, "pharmacy-documents", baseUrl),
                OwnerNationalIdBack = DocumentSetting.GetFileUrl(application.OwnerNationalIdBack, "pharmacy-documents", baseUrl),
                SelfieWithId = DocumentSetting.GetFileUrl(application.SelfieWithId, "pharmacy-documents", baseUrl),
                SyndicateCard = DocumentSetting.GetFileUrl(application.SyndicateCard, "pharmacy-documents", baseUrl),
                Status = application.Status.ToString(),
                AppliedAt = application.AppliedAt,
                RejectionReason = application.RejectionReason,
            };
        }

        public async Task<ApplicationReviewResponseDto> ReviewPharmacyApplicationAsync(Guid id, ReviewPharmacyApplicationDto dto)
        {
            var application = await _unitOfWork.Repository<PharmacyApply, Guid>().GetAsync(id);
            if (application == null)
                throw new KeyNotFoundException("Application not found");

            if (application.Status != ApplicationStatus.Pending)
                throw new InvalidOperationException($"Application already {application.Status}");

            // Validate status
            if (dto.Status != ApplicationStatus.Approved && dto.Status != ApplicationStatus.Rejected)
                throw new InvalidOperationException("Status must be either 'Approved' or 'Rejected'");

            // Get user
            var user = await _userManager.FindByIdAsync(application.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            //Check if user already has any role
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                // Delete the application because user already has a role
                _unitOfWork.Repository<PharmacyApply, Guid>().Delete(application);
                    await _unitOfWork.CompleteAsync();

                    throw new InvalidOperationException("User already has a role and cannot apply for the service.");
                }
            

            // Update application
            application.Status = dto.Status;
            application.RejectionReason = dto.RejectionReason;

            _unitOfWork.Repository<PharmacyApply, Guid>().Update(application);

            if (dto.Status == ApplicationStatus.Approved)
            {
                // Create pharmacy profile
                var profile = new PharmacyProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = application.UserId,
                    Latitude = 0,
                    Longitude = 0,
                    PharmacyName = application.PharmacyName,
                    Address = application.Address,
                    Phone = application.Phone,
                    Description = string.Empty,
                    WorkingHours = "9 AM - 9 PM",
                    IsActive = true,
                    Specializations = string.Empty,
                    AverageRating = 0,
                    TotalRatings = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<PharmacyProfile, Guid>().AddAsync(profile);

                // Add Pharmacy role to user
                if (user != null)
                {
                    var roleExists = await _userManager.IsInRoleAsync(user, "Pharmacy");
                    if (!roleExists)
                    {
                        await _userManager.AddToRoleAsync(user, "Pharmacy");
                    }
                }
            }

            await _unitOfWork.CompleteAsync();

            return new ApplicationReviewResponseDto
            {
                Success = true,
                Message = dto.Status == ApplicationStatus.Approved
                    ? "Application approved and pharmacy profile created successfully"
                    : "Application rejected",
                Status = dto.Status.ToString()
            };
        }
    }
}
