using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Admin;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Entities.Accessories;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Community;
using CoreLayer.Entities.Doctors;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Helper.Documents;
using CoreLayer.Service_Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AdminService(
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

        // ==================== SPECIES MANAGEMENT ====================

        public async Task<SpeciesResponseDto> AddSpeciesAsync(SpeciesAdminDto dto)
        {
            // Check ALL species including soft-deleted ones
            var existingSpecies = await _unitOfWork.Repository<Species, int>()
                .FindAsync(s => s.Name.ToLower() == dto.Name.ToLower());

            var existingSpeciesItem = existingSpecies.FirstOrDefault();

            if (existingSpeciesItem != null)
            {
                if (!existingSpeciesItem.IsActive)
                {
                    // Reactivate soft-deleted species
                    existingSpeciesItem.IsActive = true;
                    _unitOfWork.Repository<Species, int>().Update(existingSpeciesItem);
                    await _unitOfWork.CompleteAsync();

                    return new SpeciesResponseDto
                    {
                        Id = existingSpeciesItem.Id,
                        Name = existingSpeciesItem.Name,
                        Message = "Species reactivated successfully"
                    };
                }

                throw new InvalidOperationException("Species already exists and is active");
            }

            // Create new species
            var species = new Species
            {
                Name = dto.Name,
                IsActive = true
            };

            await _unitOfWork.Repository<Species, int>().AddAsync(species);
            await _unitOfWork.CompleteAsync();

            return new SpeciesResponseDto
            {
                Id = species.Id,
                Name = species.Name,
                Message = "Species added successfully"
            };
        }

        public async Task<DeleteResponseDto> DeleteSpeciesAsync(int id)
        {
            var species = await _unitOfWork.Repository<Species, int>().GetAsync(id);
            if (species == null)
                throw new KeyNotFoundException("Species not found or already deleted");

            // Check for ACTIVE related entities only
            var activeAnimals = await _unitOfWork.Repository<Animal, int>()

                .FindAsync(a => a.SpeciesId == id);
            if (activeAnimals.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete species. It has {activeAnimals.Count()} active animal(s). " +
                    "Please delete or reassign these animals first.");
            }

            var activeListings = await _unitOfWork.Repository<AccessoryListing, int>()
                .FindAsync(al => al.SpeciesId == id);

            if (activeListings.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete species. It has {activeListings.Count()} active accessory listing(s).");
            }

            var activePharmacyListings = await _unitOfWork.Repository<PharmacyListing, int>()
                .FindAsync(pl => pl.SpeciesId == id);

            if (activePharmacyListings.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete species. It has {activePharmacyListings.Count()} active pharmacy listing(s).");
            }

            var activeProducts = await _unitOfWork.Repository<Product, int>()
                .FindAsync(p => p.SpeciesId == id);

            if (activeProducts.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete species. It has {activeProducts.Count()} active product(s).");
            }

            var activePosts = await _unitOfWork.Repository<Post, int>()
                .FindAsync(p => p.SpeciesId == id);

            if (activePosts.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete species. It has {activePosts.Count()} active post(s).");
            }

            // Soft delete
            species.IsActive = false;
            _unitOfWork.Repository<Species, int>().Update(species);
            await _unitOfWork.CompleteAsync();

            return new DeleteResponseDto
            {
                Success = true,
                Message = "Species deleted successfully",
                DeletedId = id
            };
        }

        // ==================== SUBSPECIES MANAGEMENT ====================

        public async Task<SubSpeciesResponseDto> AddSubSpeciesAsync(SubSpeciesAdminDto dto)
        {
            // Validate parent species is active
            var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId);
            if (species == null)
                throw new KeyNotFoundException("Species not found or inactive");

            if (!species.IsActive)
                throw new InvalidOperationException("Cannot add SubSpecies to an inactive Species");


            // Check ALL subspecies including soft-deleted ones
            var existingSubSpecies = await _unitOfWork.Repository<SubSpecies, int>()
                .FindAsync(ss => ss.Name.ToLower() == dto.Name.ToLower() && ss.SpeciesId == dto.SpeciesId);

            var existingSubSpeciesItem = existingSubSpecies.FirstOrDefault();

            if (existingSubSpeciesItem != null)
            {
                if (!existingSubSpeciesItem.IsActive)
                {
                    // Reactivate soft-deleted subspecies
                    existingSubSpeciesItem.IsActive = true;
                    _unitOfWork.Repository<SubSpecies, int>().Update(existingSubSpeciesItem);
                    await _unitOfWork.CompleteAsync();

                    return new SubSpeciesResponseDto
                    {
                        Id = existingSubSpeciesItem.Id,
                        Name = existingSubSpeciesItem.Name,
                        SpeciesId = existingSubSpeciesItem.SpeciesId,
                        SpeciesName = species.Name,
                        Message = "SubSpecies reactivated successfully"
                    };
                }

                throw new InvalidOperationException("SubSpecies already exists and is active for this species");
            }

            // Create new subspecies
            var subSpecies = new SubSpecies
            {
                Name = dto.Name,
                SpeciesId = dto.SpeciesId,
                IsActive = true
            };

            await _unitOfWork.Repository<SubSpecies, int>().AddAsync(subSpecies);
            await _unitOfWork.CompleteAsync();

            return new SubSpeciesResponseDto
            {
                Id = subSpecies.Id,
                Name = subSpecies.Name,
                SpeciesId = subSpecies.SpeciesId,
                SpeciesName = species.Name,
                Message = "SubSpecies added successfully"
            };
        }

        public async Task<DeleteResponseDto> DeleteSubSpeciesAsync(int id)
        {
            var subSpecies = await _unitOfWork.Repository<SubSpecies, int>().GetAsync(id);
            if (subSpecies == null)
                throw new KeyNotFoundException("SubSpecies not found or already deleted");

            // Check for active related animals (global filter handles IsActive check)
            var activeAnimals = await _unitOfWork.Repository<Animal, int>()
                .FindAsync(a => a.SubSpeciesId == id);

            if (activeAnimals.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete subspecies. It has {activeAnimals.Count()} active animal(s). " +
                    "Please delete or reassign these animals first.");
            }

            // Soft delete
            subSpecies.IsActive = false;
            _unitOfWork.Repository<SubSpecies, int>().Update(subSpecies);
            await _unitOfWork.CompleteAsync();

            return new DeleteResponseDto
            {
                Success = true,
                Message = "SubSpecies deleted successfully",
                DeletedId = id
            };
        }

        // ==================== COLOR MANAGEMENT ====================

        public async Task<ColorResponseDto> AddColorAsync(ColorAdminDto dto)
        {
            // Check ALL colors including soft-deleted ones
            var existingColor = await _unitOfWork.Repository<Color, int>()
                .FindAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            var existingColorItem = existingColor.FirstOrDefault();

            if (existingColorItem != null)
            {
                if (!existingColorItem.IsActive)
                {
                    // Reactivate soft-deleted color
                    existingColorItem.IsActive = true;
                    _unitOfWork.Repository<Color, int>().Update(existingColorItem);
                    await _unitOfWork.CompleteAsync();

                    return new ColorResponseDto
                    {
                        Id = existingColorItem.Id,
                        Name = existingColorItem.Name,
                        Message = "Color reactivated successfully"
                    };
                }

                throw new InvalidOperationException("Color already exists and is active");
            }

            // Create new color
            var color = new Color
            {
                Name = dto.Name,
                IsActive = true
            };

            await _unitOfWork.Repository<Color, int>().AddAsync(color);
            await _unitOfWork.CompleteAsync();

            return new ColorResponseDto
            {
                Id = color.Id,
                Name = color.Name,
                Message = "Color added successfully"
            };
        }

        public async Task<DeleteResponseDto> DeleteColorAsync(int id)
        {
            var color = await _unitOfWork.Repository<Color, int>().GetAsync(id);
            if (color == null)
                throw new KeyNotFoundException("Color not found or already deleted");

            // Check for active related animals (global filter handles IsActive check)
            var activeAnimals = await _unitOfWork.Repository<Animal, int>()
                .FindAsync(a => a.ColorId == id);

            if (activeAnimals.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete color. It has {activeAnimals.Count()} active animal(s). " +
                    "Please delete or reassign these animals first.");
            }

            // Soft delete
            color.IsActive = false;
            _unitOfWork.Repository<Color, int>().Update(color);
            await _unitOfWork.CompleteAsync();

            return new DeleteResponseDto
            {
                Success = true,
                Message = "Color deleted successfully",
                DeletedId = id
            };
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
                    WorkingHours ="9 AM - 5 PM",
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
                var user = await _userManager.FindByIdAsync(application.UserId);
                if (user != null)
                {
                    var roleExists = await _userManager.IsInRoleAsync(user, "Doctor");
                    if (!roleExists)
                    {
                        await _userManager.AddToRoleAsync(user, "Doctor");
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