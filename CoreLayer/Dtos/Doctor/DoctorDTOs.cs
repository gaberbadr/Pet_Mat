using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CoreLayer.Dtos.Doctor
{
    // ==================== INPUT DTOs ====================

    public class DoctorFilterParams
    { public string? Specialization { get; set; }
        public int? MinExperienceYears { get; set; }
        public string? City { get; set; } 
        public string? Government { get; set; } 
        public string? Search { get; set; } 
        public int PageIndex { get; set; } = 1; 
        public int PageSize { get; set; } = 10; }

    public class ApplyDoctorDto
    {
        [Required]
        [MaxLength(200)]
        public string Specialization { get; set; }

        [Required]
        [Range(0, 100)]
        public int ExperienceYears { get; set; }

        [Required]
        [MaxLength(500)]
        public string ClinicAddress { get; set; }

        [Required]
        public IFormFile NationalIdFront { get; set; }

        [Required]
        public IFormFile NationalIdBack { get; set; }

        [Required]
        public IFormFile SelfieWithId { get; set; }

        [Required]
        public IFormFile SyndicateCard { get; set; }

        [Required]
        public IFormFile MedicalLicense { get; set; }
    }

    public class ReviewDoctorApplicationDto
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // "Approved" or "Rejected"

        public string? RejectionReason { get; set; }

    }
    public class UpdateDoctorLocationDto
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
    public class UpdateDoctorProfileDto
    {

        [MaxLength(200)]
        public string? Specialization { get; set; }

        [Range(0, 100)]
        public int? ExperienceYears { get; set; }

        [MaxLength(500)]
        public string? ClinicAddress { get; set; }

        [MaxLength(200)]
        public string? ClinicName { get; set; }

        public string? Bio { get; set; }

        public string? WorkingHours { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public bool? IsAvailableForConsultation { get; set; }

        public string? Services { get; set; }

        public string? Languages { get; set; }
    }

    public class RateDoctorDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Review { get; set; }

        [Required]
        [Range(1, 5)]
        public int CommunicationRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int KnowledgeRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int ResponsivenessRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int ProfessionalismRating { get; set; }
    }

    // ==================== OUTPUT DTOs ====================

    public class DoctorApplicationListDto
    {
        public int Count { get; set; }
        public IEnumerable<DoctorApplicationSummaryDto> Data { get; set; }
    }

    public class DoctorApplicationSummaryDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class DoctorApplicationDetailDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public string ClinicAddress { get; set; }
        public string NationalIdFront { get; set; }
        public string NationalIdBack { get; set; }
        public string SelfieWithId { get; set; }
        public string SyndicateCard { get; set; }
        public string MedicalLicense { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
    public class UserDoctorApplicationStatusDto
    {
        public Guid ApplicationId { get; set; }
        public string Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class DoctorProfileResponseDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public string ClinicAddress { get; set; }
        public string ClinicName { get; set; }
        public string Bio { get; set; }
        public string WorkingHours { get; set; }
        public string Phone { get; set; }
        public bool IsAvailableForConsultation { get; set; }
        public bool IsActive { get; set; }
        public string Services { get; set; }
        public string Languages { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PublicDoctorProfileDto
    {
        public Guid Id { get; set; }
        public string DoctorName { get; set; }
        public string ProfilePicture { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public string ClinicAddress { get; set; }
        public string ClinicName { get; set; }
        public string Bio { get; set; }
        public string WorkingHours { get; set; }
        public string Phone { get; set; }
        public bool IsAvailableForConsultation { get; set; }
        public string Services { get; set; }
        public string Languages { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public string City { get; set; }
        public string Government { get; set; }
        public IEnumerable<DoctorRatingDto> RecentRatings { get; set; }
    }

    public class DoctorRatingListDto
    {
        public int Count { get; set; }
        public double AverageRating { get; set; }
        public IEnumerable<DoctorRatingDto> Data { get; set; }
    }

    public class DoctorRatingDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
        public int Rating { get; set; }
        public string Review { get; set; }
        public int CommunicationRating { get; set; }
        public int KnowledgeRating { get; set; }
        public int ResponsivenessRating { get; set; }
        public int ProfessionalismRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DoctorApplicationOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid? ApplicationId { get; set; }
    }

    public class ApplicationReviewResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }

    public class DoctorProfileOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class RatingOperationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? RatingId { get; set; }
    }
}
