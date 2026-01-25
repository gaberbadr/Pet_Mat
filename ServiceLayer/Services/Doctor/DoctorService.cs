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
using CoreLayer.Service_Interface.Doctor;
using Microsoft.AspNetCore.Identity;

namespace ServiceLayer.Services.Doctor
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        // ==================== PROFILE MANAGEMENT ====================

        public async Task<DoctorProfileResponseDto> GetDoctorProfileAsync(string userId)
        {
            var profile = (await _unitOfWork.Repository<DoctorProfile, Guid>()
                .FindAsync(dp => dp.UserId == userId && dp.IsActive)).FirstOrDefault();

            if (profile == null)
                throw new KeyNotFoundException("Doctor profile not found");

            return _mapper.Map<DoctorProfileResponseDto>(profile);
        }

        public async Task<DoctorProfileOperationResponseDto> UpdateDoctorProfileAsync(string userId, UpdateDoctorProfileDto dto)
        {
            var profile = (await _unitOfWork.Repository<DoctorProfile, Guid>()
                .FindAsync(dp => dp.UserId == userId && dp.IsActive)).FirstOrDefault();

            if (profile == null)
                throw new KeyNotFoundException("Doctor profile not found");

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.Specialization)) profile.Specialization = dto.Specialization;
            if (dto.ExperienceYears.HasValue) profile.ExperienceYears = dto.ExperienceYears.Value;
            if (!string.IsNullOrEmpty(dto.ClinicAddress)) profile.ClinicAddress = dto.ClinicAddress;
            if (!string.IsNullOrEmpty(dto.ClinicName)) profile.ClinicName = dto.ClinicName;
            if (!string.IsNullOrEmpty(dto.Bio)) profile.Bio = dto.Bio;
            if (!string.IsNullOrEmpty(dto.WorkingHours)) profile.WorkingHours = dto.WorkingHours;
            if (!string.IsNullOrEmpty(dto.Phone)) profile.Phone = dto.Phone;
            if (dto.IsAvailableForConsultation.HasValue)
                profile.IsAvailableForConsultation = dto.IsAvailableForConsultation.Value;
            if (!string.IsNullOrEmpty(dto.Services)) profile.Services = dto.Services;
            if (!string.IsNullOrEmpty(dto.Languages)) profile.Languages = dto.Languages;

            _unitOfWork.Repository<DoctorProfile, Guid>().Update(profile);
            await _unitOfWork.CompleteAsync();

            return new DoctorProfileOperationResponseDto
            {
                Success = true,
                Message = "Doctor profile updated successfully"
            };
        }

        public async Task<DoctorProfileOperationResponseDto> UpdateDoctorLocationAsync(string userId, UpdateDoctorLocationDto dto)
        {
            var profile = (await _unitOfWork.Repository<DoctorProfile, Guid>()
                .FindAsync(dp => dp.UserId == userId && dp.IsActive)).FirstOrDefault();

            if (profile == null)
                throw new KeyNotFoundException("Doctor profile not found");

            // Update location
            profile.Latitude = dto.Latitude;
            profile.Longitude = dto.Longitude;

            _unitOfWork.Repository<DoctorProfile, Guid>().Update(profile);
            await _unitOfWork.CompleteAsync();

            return new DoctorProfileOperationResponseDto
            {
                Success = true,
                Message = "Location updated successfully"
            };
        }

        public async Task<DoctorProfileOperationResponseDto> DeleteDoctorAccountAsync(string userId)
        {
            // Get user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Delete doctor ratings
            var ratings = await _unitOfWork.Repository<DoctorRating, int>()
                .FindAsync(dr => dr.DoctorId == userId);

            foreach (var rating in ratings)
            {
                _unitOfWork.Repository<DoctorRating, int>().Delete(rating);
            }

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

            // Remove Doctor role
            var isDoctor = await _userManager.IsInRoleAsync(user, "Doctor");
            if (isDoctor)
            {
                await _userManager.RemoveFromRoleAsync(user, "Doctor");
            }


            await _unitOfWork.CompleteAsync();

            return new DoctorProfileOperationResponseDto
            {
                Success = true,
                Message = "Doctor account deleted successfully. Your doctor role has been removed."
            };
        }

        // ==================== RATINGS ====================

        public async Task<DoctorRatingListDto> GetDoctorRatingsAsync(string doctorId)
        {
            var ratings = await _unitOfWork.Repository<DoctorRating, int>()
                .FindAsync(dr => dr.DoctorId == doctorId);

            var ratingDtos = new List<DoctorRatingDto>();

            foreach (var rating in ratings.OrderByDescending(r => r.CreatedAt))
            {
                var user = await _userManager.FindByIdAsync(rating.UserId);
                if (user != null)
                {
                    ratingDtos.Add(new DoctorRatingDto
                    {
                        Id = rating.Id,
                        UserName = $"{user.FirstName} {user.LastName}",
                        UserProfilePicture = user.ProfilePicture,
                        Rating = rating.Rating,
                        Review = rating.Review,
                        CommunicationRating = rating.CommunicationRating,
                        KnowledgeRating = rating.KnowledgeRating,
                        ResponsivenessRating = rating.ResponsivenessRating,
                        ProfessionalismRating = rating.ProfessionalismRating,
                        CreatedAt = rating.CreatedAt
                    });
                }
            }

            var averageRating = ratingDtos.Any() ? ratingDtos.Average(r => r.Rating) : 0;

            return new DoctorRatingListDto
            {
                Count = ratingDtos.Count,
                AverageRating = Math.Round(averageRating, 2),
                Data = ratingDtos
            };
        }
    }
}
