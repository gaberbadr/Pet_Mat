using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Auth;
using Microsoft.AspNetCore.Http;

namespace CoreLayer.Service_Interface.IAuth
{
    public interface IAuthUserService
    {
        Task<(bool Success, string Message, TokenResponseDto? Token, int? BanMinutes)> LoginAsync(string email, string password, string? ipAddress);
        Task<(bool Success, string Message)> CreatePasswordAsync(string userId, string password);
        Task<(bool Success, string Message)> UpdatePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<(bool Success, string Message)> UpdateNameAsync(string userId, string firstName, string lastName);
        Task<(bool Success, string Message)> UpdatePhoneAsync(string userId, string phoneNumber);
        Task<(bool Success, string Message, string? PictureUrl)> UpdateProfilePictureAsync(string userId, IFormFile picture, string baseUrl);
        Task<(bool Success, string Message)> CreateOrUpdateAddressAsync(string userId, AddressDto dto);
        Task<(bool Success, string Message, PublicUserProfileDto? Profile)> GetPublicUserProfileAsync(
         string userId, string baseUrl);
        Task<(bool Success, string Message, UserProfileDto? Profile)> GetUserProfileAsync(string userId, string baseUrl);
    }
}
