using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CoreLayer.Dtos.Auth
{

    public class PublicUserProfileDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string ProfilePhotoUrl { get; set; }
        public string City { get; set; }
        public string Government { get; set; }
        public DateTime CreatedAt { get; set; }

        // Role information
        public bool IsDoctor { get; set; }
        public bool IsPharmacy { get; set; }

        // Doctor-specific info (if applicable)
        public string Specialization { get; set; }
        public int? ExperienceYears { get; set; }
        public double? DoctorAverageRating { get; set; }
        public int? DoctorTotalRatings { get; set; }

        // Pharmacy-specific info (if applicable)
        public string PharmacyName { get; set; }
        public double? PharmacyAverageRating { get; set; }
        public int? PharmacyTotalRatings { get; set; }

        // Activity stats
        public int TotalAnimals { get; set; }
        public int TotalListings { get; set; }
        public int TotalPosts { get; set; }
    }
    public class EmailDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }

    public class VerifyCodeDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
    }

    public class CreatePasswordDto
    {
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    public class UpdatePasswordDto
    {
        [Required(ErrorMessage = "Old password is required")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; }
    }

    public class UpdateNameDto
    {
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; }
    }

    public class UpdatePhoneDto
    {
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }
    }

    public class UpdateProfilePictureDto
    {
        [Required(ErrorMessage = "Profile picture is required")]
        public IFormFile Picture { get; set; }
    }

    public class AddressDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; }

        [Required(ErrorMessage = "Government is required")]
        [MaxLength(100, ErrorMessage = "Government cannot exceed 100 characters")]
        public string Government { get; set; }
    }

    public class RefreshRequestDto
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; }
    }

    // ========== Response DTOs ==========

    public class TokenResponseDto
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresAt { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
    }

    public class UserProfileDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
        public bool HasPassword { get; set; }
        public string ProfilePhotoUrl { get; set; }

        public AddressDto? Address { get; set; }
    }
}
