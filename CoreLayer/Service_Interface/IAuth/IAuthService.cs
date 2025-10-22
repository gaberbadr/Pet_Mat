using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Auth;
using CoreLayer.Entities.Identity;
using Microsoft.AspNetCore.Http;

namespace CoreLayer.Service_Interface.IAuth
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> SendVerificationCodeAsync(string email);
        Task<(bool Success, string Message, TokenResponseDto? Token)> VerifyCodeAsync(string email, string code);
        Task<(bool Success, string Message, TokenResponseDto? Token)> RefreshTokenAsync(string refreshToken, string? ipAddress);
        Task<(bool Success, string Message)> RevokeTokenAsync(string refreshToken);
        Task<(bool Success, string Message)> LogoutAsync(string userId);
        Task<(ApplicationUser? User, string ErrorMessage)> HandleGoogleCallbackAsync(string? ipAddress);
        Task<string> GenerateTokenQueryStringAsync(ApplicationUser user, string? ipAddress);
    }
}
