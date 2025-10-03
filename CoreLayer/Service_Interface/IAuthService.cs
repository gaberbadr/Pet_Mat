using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Auth;
using CoreLayer.Entities.Identity;
using Microsoft.AspNetCore.Http;

namespace CoreLayer.Service_Interface
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> SendVerificationCodeAsync(string email);
        Task<(bool Success, string Message, TokenResponseDto? Token)> VerifyCodeAsync(string email, string code);
        Task<(bool Success, string Message, TokenResponseDto? Token, int? BanMinutes)> LoginAsync(string email, string password, string? ipAddress);
        Task<(bool Success, string Message)> CreatePasswordAsync(string userId, string password);
        Task<(bool Success, string Message)> UpdatePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<(bool Success, string Message)> UpdateNameAsync(string userId, string firstName, string lastName);
        Task<(bool Success, string Message)> UpdatePhoneAsync(string userId, string phoneNumber);
        Task<(bool Success, string Message, string? PictureUrl)> UpdateProfilePictureAsync(string userId, IFormFile picture, string baseUrl);
        Task<(bool Success, string Message)> CreateOrUpdateAddressAsync(string userId, AddressDto dto);
        Task<(bool Success, string Message, UserProfileDto? Profile)> GetUserProfileAsync(string userId, string baseUrl);
        Task<(bool Success, string Message, TokenResponseDto? Token)> RefreshTokenAsync(string refreshToken, string? ipAddress);
        Task<(bool Success, string Message)> RevokeTokenAsync(string refreshToken);
        Task<(bool Success, string Message)> LogoutAsync(string userId);
        Task<(ApplicationUser? User, string ErrorMessage)> HandleGoogleCallbackAsync(string? ipAddress);
        Task<string> GenerateTokenQueryStringAsync(ApplicationUser user, string? ipAddress);
    }
}
