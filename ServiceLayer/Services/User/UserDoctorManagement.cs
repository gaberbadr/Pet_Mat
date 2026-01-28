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
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface.User;
using CoreLayer.Specifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.User
{
    public class UserDoctorManagement : IUserDoctorManagement
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserDoctorManagement(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _userManager = userManager;
        }

        // ==================== DOCTOR ====================
        //get all doctors with filtering and pagination
        public async Task<PaginationResponse<PublicDoctorProfileDto>> GetDoctorsAsync(DoctorFilterParams filterParams)
        {
            // Build specifications 
            var spec = new DoctorFilterSpecification(filterParams);
            var countSpec = new DoctorFilterCountSpecification(filterParams);

            // Get total count 
            var totalCount = await _unitOfWork.Repository<DoctorProfile, Guid>().GetCountAsync(countSpec);

            // Get filtered + paginated doctors
            var doctors = await _unitOfWork.Repository<DoctorProfile, Guid>().GetAllWithSpecficationAsync(spec);

            // Map to DTOs
            var doctorDtos = doctors.Select(dp => new PublicDoctorProfileDto
            {
                Id = dp.Id,
                UserId = dp.UserId,
                DoctorName = $"{dp.User.FirstName} {dp.User.LastName}",
                ProfilePicture = dp.User.ProfilePicture,
                Specialization = dp.Specialization,
                ExperienceYears = dp.ExperienceYears,
                ClinicAddress = dp.ClinicAddress,
                ClinicName = dp.ClinicName,
                AverageRating = dp.AverageRating,
                TotalRatings = dp.TotalRatings,
                City = dp.User?.Address?.City,
                Government = dp.User?.Address?.Government,
                RecentRatings = dp.Ratings
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r =>
                    {
                        var user = _userManager.FindByIdAsync(r.UserId).Result;
                        return new DoctorRatingDto
                        {
                            Id = r.Id,
                            UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                            UserProfilePicture = user?.ProfilePicture,
                            Rating = r.Rating,
                            Review = r.Review,
                            CommunicationRating = r.CommunicationRating,
                            KnowledgeRating = r.KnowledgeRating,
                            ResponsivenessRating = r.ResponsivenessRating,
                            ProfessionalismRating = r.ProfessionalismRating,
                            CreatedAt = r.CreatedAt
                        };
                    })
                    .ToList()
            }).ToList();

            // Return paginated response
            return new PaginationResponse<PublicDoctorProfileDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                doctorDtos
            );
        }

        // ==================== DOCTOR APPLICATION (USER) ====================

        public async Task<DoctorApplicationOperationResponseDto> ApplyToBeDoctorAsync(ApplyDoctorDto dto, string userId)
        {
            // Check if user already has a role
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Any())
                throw new InvalidOperationException("You already have a role and cannot apply to be a doctor");

            // Delete any existing application
            var existingApplication = (await _unitOfWork.Repository<DoctorApply, Guid>()
                .FindAsync(da => da.UserId == userId)).FirstOrDefault();

            if (existingApplication != null)
            {
                // Delete old documents
                if (!string.IsNullOrEmpty(existingApplication.NationalIdFront))
                    DocumentSetting.Delete(existingApplication.NationalIdFront, "doctor-documents");
                if (!string.IsNullOrEmpty(existingApplication.NationalIdBack))
                    DocumentSetting.Delete(existingApplication.NationalIdBack, "doctor-documents");
                if (!string.IsNullOrEmpty(existingApplication.SelfieWithId))
                    DocumentSetting.Delete(existingApplication.SelfieWithId, "doctor-documents");
                if (!string.IsNullOrEmpty(existingApplication.SyndicateCard))
                    DocumentSetting.Delete(existingApplication.SyndicateCard, "doctor-documents");
                if (!string.IsNullOrEmpty(existingApplication.MedicalLicense))
                    DocumentSetting.Delete(existingApplication.MedicalLicense, "doctor-documents");

                _unitOfWork.Repository<DoctorApply, Guid>().Delete(existingApplication);
                await _unitOfWork.CompleteAsync();
            }

            // Upload new documents
            var nationalIdFront = DocumentSetting.Upload(dto.NationalIdFront, "doctor-documents");
            var nationalIdBack = DocumentSetting.Upload(dto.NationalIdBack, "doctor-documents");
            var selfieWithId = DocumentSetting.Upload(dto.SelfieWithId, "doctor-documents");
            var syndicateCard = DocumentSetting.Upload(dto.SyndicateCard, "doctor-documents");
            var medicalLicense = DocumentSetting.Upload(dto.MedicalLicense, "doctor-documents");

            // Create new application
            var application = new DoctorApply
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Specialization = dto.Specialization,
                ExperienceYears = dto.ExperienceYears,
                ClinicAddress = dto.ClinicAddress,
                NationalIdFront = nationalIdFront,
                NationalIdBack = nationalIdBack,
                SelfieWithId = selfieWithId,
                SyndicateCard = syndicateCard,
                MedicalLicense = medicalLicense,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<DoctorApply, Guid>().AddAsync(application);
            await _unitOfWork.CompleteAsync();

            return new DoctorApplicationOperationResponseDto
            {
                Success = true,
                Message = "Doctor application submitted successfully",
                ApplicationId = application.Id
            };
        }

        public async Task<UserDoctorApplicationStatusDto> GetDoctorApplicationStatusAsync(string userId)
        {
            // Check if user already has doctor role
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var isDoctor = await _userManager.IsInRoleAsync(user, "Doctor");
            if (isDoctor)
                throw new InvalidOperationException("You are already a doctor and cannot view application status");

            // Get application
            var application = (await _unitOfWork.Repository<DoctorApply, Guid>()
                .FindAsync(da => da.UserId == userId)).FirstOrDefault();

            if (application == null)
                throw new KeyNotFoundException("No doctor application found");

            return new UserDoctorApplicationStatusDto
            {
                ApplicationId = application.Id,
                Status = application.Status,
                AppliedAt = application.AppliedAt,
                RejectionReason = application.RejectionReason
            };
        }
        // ==================== DOCTOR RATING (USER) ====================

        public async Task<RatingOperationResponseDto> RateDoctorAsync(string doctorId, RateDoctorDto dto, string userId)
        {
            // Verify doctor exists and has doctor role
            var doctor = await _userManager.FindByIdAsync(doctorId);
            if (doctor == null)
                throw new KeyNotFoundException("Doctor not found");

            var isDoctor = await _userManager.IsInRoleAsync(doctor, "Doctor");
            if (!isDoctor)
                throw new KeyNotFoundException("User is not a doctor");

            // Check if user already rated this doctor
            var existingRating = (await _unitOfWork.Repository<DoctorRating, int>()
                .FindAsync(dr => dr.DoctorId == doctorId && dr.UserId == userId)).FirstOrDefault();

            if (existingRating != null)
                throw new InvalidOperationException("You have already rated this doctor. Use update endpoint to modify your rating.");

            // Create rating
            var rating = new DoctorRating
            {
                DoctorId = doctorId,
                UserId = userId,
                Rating = dto.Rating,
                Review = dto.Review,
                CommunicationRating = dto.CommunicationRating,
                KnowledgeRating = dto.KnowledgeRating,
                ResponsivenessRating = dto.ResponsivenessRating,
                ProfessionalismRating = dto.ProfessionalismRating,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<DoctorRating, int>().AddAsync(rating);

            // Update doctor profile average rating
            var profile = (await _unitOfWork.Repository<DoctorProfile, Guid>()
                .FindAsync(dp => dp.UserId == doctorId && dp.IsActive)).FirstOrDefault();

            if (profile != null)
            {
                var allRatings = await _unitOfWork.Repository<DoctorRating, int>()
                    .FindAsync(dr => dr.DoctorId == doctorId);

                var ratingsList = allRatings.ToList();
                ratingsList.Add(rating); // Include the new rating

                profile.TotalRatings = ratingsList.Count;
                profile.AverageRating = Math.Round(ratingsList.Average(r => r.Rating), 2);

                _unitOfWork.Repository<DoctorProfile, Guid>().Update(profile);
            }

            await _unitOfWork.CompleteAsync();

            return new RatingOperationResponseDto
            {
                Success = true,
                Message = "Doctor rated successfully",
                RatingId = rating.Id
            };
        }

        public async Task<RatingOperationResponseDto> UpdateDoctorRatingAsync(string doctorId, RateDoctorDto dto, string userId)
        {
            // Get existing rating
            var rating = (await _unitOfWork.Repository<DoctorRating, int>()
                .FindAsync(dr => dr.DoctorId == doctorId && dr.UserId == userId)).FirstOrDefault();

            if (rating == null)
                throw new KeyNotFoundException("Rating not found");

            if (rating.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own ratings");

            // Update rating
            rating.Rating = dto.Rating;
            rating.Review = dto.Review;
            rating.CommunicationRating = dto.CommunicationRating;
            rating.KnowledgeRating = dto.KnowledgeRating;
            rating.ResponsivenessRating = dto.ResponsivenessRating;
            rating.ProfessionalismRating = dto.ProfessionalismRating;

            _unitOfWork.Repository<DoctorRating, int>().Update(rating);

            // Update doctor profile average rating
            var profile = (await _unitOfWork.Repository<DoctorProfile, Guid>()
                .FindAsync(dp => dp.UserId == doctorId && dp.IsActive)).FirstOrDefault();

            if (profile != null)
            {
                var allRatings = await _unitOfWork.Repository<DoctorRating, int>()
                    .FindAsync(dr => dr.DoctorId == doctorId);

                var ratingsList = allRatings.ToList();
                profile.AverageRating = Math.Round(ratingsList.Average(r => r.Rating), 2);

                _unitOfWork.Repository<DoctorProfile, Guid>().Update(profile);
            }

            await _unitOfWork.CompleteAsync();

            return new RatingOperationResponseDto
            {
                Success = true,
                Message = "Rating updated successfully",
                RatingId = rating.Id
            };
        }


        public async Task<PublicDoctorProfileDto> GetPublicDoctorProfileAsync(string doctorId)
        {
            // Verify doctor exists
            var doctor = await _userManager.FindByIdAsync(doctorId);
            if (doctor == null)
                throw new KeyNotFoundException("Doctor not found");

            var isDoctor = await _userManager.IsInRoleAsync(doctor, "Doctor");
            if (!isDoctor)
                throw new KeyNotFoundException("User is not a doctor");

            // Use specification to get doctor profile with User and Ratings included
            var spec = new DoctorByUserIdSpecification(doctorId);
            var profile = await _unitOfWork.Repository<DoctorProfile, Guid>().GetWithSpecficationAsync(spec);

            if (profile == null)
                throw new KeyNotFoundException("Doctor profile not found");

            // Get recent ratings
            var recentRatings = profile.Ratings
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r =>
                {
                    var user = _userManager.FindByIdAsync(r.UserId).Result;
                    return new DoctorRatingDto
                    {
                        Id = r.Id,
                        UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                        UserProfilePicture = user?.ProfilePicture,
                        Rating = r.Rating,
                        Review = r.Review,
                        CommunicationRating = r.CommunicationRating,
                        KnowledgeRating = r.KnowledgeRating,
                        ResponsivenessRating = r.ResponsivenessRating,
                        ProfessionalismRating = r.ProfessionalismRating,
                        CreatedAt = r.CreatedAt
                    };
                }).ToList();

            return new PublicDoctorProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                ProfilePicture = doctor.ProfilePicture,
                Latitude = (double)profile.Latitude,
                Longitude = (double)profile.Longitude,
                Specialization = profile.Specialization,
                ExperienceYears = profile.ExperienceYears,
                ClinicAddress = profile.ClinicAddress,
                ClinicName = profile.ClinicName,
                Bio = profile.Bio,
                WorkingHours = profile.WorkingHours,
                Phone = profile.Phone,
                IsAvailableForConsultation = profile.IsAvailableForConsultation,
                Services = profile.Services,
                Languages = profile.Languages,
                AverageRating = profile.AverageRating,
                TotalRatings = profile.TotalRatings,
                City = profile.User?.Address?.City,
                Government = profile.User?.Address?.Government,
                RecentRatings = recentRatings
            };
        }

        // ==================== GET ALL DOCTOR RATINGS ====================

        public async Task<DoctorRatingListDto> GetDoctorAllRatingsAsync(string doctorId)
        {
            // Verify doctor exists
            var doctor = await _userManager.FindByIdAsync(doctorId);
            if (doctor == null)
                throw new KeyNotFoundException("Doctor not found");

            var isDoctor = await _userManager.IsInRoleAsync(doctor, "Doctor");
            if (!isDoctor)
                throw new KeyNotFoundException("User is not a doctor");

            // Get all ratings for this doctor
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
