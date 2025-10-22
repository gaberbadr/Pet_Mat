using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Dtos.Auth;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Community;
using CoreLayer.Entities.Doctors;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Helper.Documents;
using CoreLayer.Service_Interface;
using CoreLayer.Service_Interface.IAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;


namespace ServiceLayer.Services.Auth.AuthUser
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IJwtService _jwtService;
        private readonly ILoginRateLimiterService _loginRateLimiter;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            IJwtService jwtService,
            ILoginRateLimiterService loginRateLimiter)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _jwtService = jwtService;
            _loginRateLimiter = loginRateLimiter;
        }

        public async Task<(bool Success, string Message)> SendVerificationCodeAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Email = email,
                        UserName = email,
                        EmailConfirmed = false
                    };
                    await _userManager.CreateAsync(user);
                }

                var code = new Random().Next(100000, 999999).ToString();
                user.VerificationCode = code;
                user.CodeExpiresAt = DateTime.UtcNow.AddMinutes(3);
                await _userManager.UpdateAsync(user);

                await _emailSender.SendEmailAsync(email, "Verification Code",
                    $"Your verification code is {code}. It will expire in 3 minute.");

                return (true, "Verification code sent successfully");
            }
            catch (Exception)
            {
                return (false, "Failed to send verification code");
            }
        }

        public async Task<(bool Success, string Message, TokenResponseDto? Token)> VerifyCodeAsync(string email, string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "Invalid email", null);

            if (user.CodeExpiresAt < DateTime.UtcNow)
                return (false, "Verification code expired", null);

            if (user.VerificationCode != code)
                return (false, "Invalid verification code", null);

            user.EmailConfirmed = true;
            user.VerificationCode = null;
            await _userManager.UpdateAsync(user);

            var tokenResponse = await GenerateTokenResponseAsync(user, null);
            return (true, "Verification successful", tokenResponse);
        }

        public async Task<(bool Success, string Message, TokenResponseDto? Token)> RefreshTokenAsync(
            string refreshToken, string? ipAddress)
        {
            var refreshTokenRepo = _unitOfWork.Repository<RefreshToken, int>();
            var tokens = await refreshTokenRepo.FindAsync(t => t.Token == refreshToken);
            var tokenEntry = tokens.FirstOrDefault();

            if (tokenEntry == null || !tokenEntry.IsActive)
                return (false, "Invalid refresh token", null);

            var user = await _userManager.FindByIdAsync(tokenEntry.UserId);
            if (user == null)
                return (false, "Invalid token user", null);

            tokenEntry.RevokedAt = DateTime.UtcNow;
            refreshTokenRepo.Update(tokenEntry);
            await _unitOfWork.CompleteAsync();

            var (accessToken, accessExp) = await _jwtService.GenerateAccessTokenAsync(user, _userManager);
            var (newRefreshToken, refreshExp) = _jwtService.GenerateRefreshToken();

            await refreshTokenRepo.AddAsync(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshExp,
                CreatedByIp = ipAddress
            });

            await refreshTokenRepo.DeleteRangeAsync(t =>
                t.UserId == user.Id && (t.ExpiresAt < DateTime.UtcNow || t.RevokedAt != null));

            await _unitOfWork.CompleteAsync();

            var tokenResponse = new TokenResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExp,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt = refreshExp
            };

            return (true, "Token refreshed successfully", tokenResponse);
        }

        public async Task<(bool Success, string Message)> RevokeTokenAsync(string refreshToken)
        {
            var refreshTokenRepo = _unitOfWork.Repository<RefreshToken, int>();
            var tokens = await refreshTokenRepo.FindAsync(t => t.Token == refreshToken);
            var tokenEntry = tokens.FirstOrDefault();

            if (tokenEntry == null)
                return (false, "Refresh token not found");

            tokenEntry.RevokedAt = DateTime.UtcNow;
            refreshTokenRepo.Update(tokenEntry);
            await _unitOfWork.CompleteAsync();

            return (true, "Refresh token revoked successfully");
        }

        public async Task<(bool Success, string Message)> LogoutAsync(string userId)
        {
            var refreshTokenRepo = _unitOfWork.Repository<RefreshToken, int>();
            await refreshTokenRepo.DeleteRangeAsync(t => t.UserId == userId);
            await _unitOfWork.CompleteAsync();

            return (true, "Logged out successfully");
        }

        public async Task<(ApplicationUser? User, string ErrorMessage)> HandleGoogleCallbackAsync(string? ipAddress)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return (null, "External login info not found");

            var externalSignIn = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            ApplicationUser user;

            if (externalSignIn.Succeeded)
            {
                user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email == null)
                    return (null, "Email not provided by Google");

                user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };
                    await _userManager.CreateAsync(user);
                }
                else if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }

                var logins = await _userManager.GetLoginsAsync(user);
                if (!logins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
                {
                    await _userManager.AddLoginAsync(user, info);
                }
            }

            return (user, string.Empty);
        }

        public async Task<string> GenerateTokenQueryStringAsync(ApplicationUser user, string? ipAddress)
        {
            var tokenResponse = await GenerateTokenResponseAsync(user, ipAddress);

            return $"accessToken={Uri.EscapeDataString(tokenResponse.AccessToken)}" +
                   $"&refreshToken={Uri.EscapeDataString(tokenResponse.RefreshToken)}" +
                   $"&accessTokenExpiresAt={Uri.EscapeDataString(tokenResponse.AccessTokenExpiresAt.ToString("O"))}" +
                   $"&refreshTokenExpiresAt={Uri.EscapeDataString(tokenResponse.RefreshTokenExpiresAt.ToString("O"))}";
        }

        // Private helper method
        private async Task<TokenResponseDto> GenerateTokenResponseAsync(ApplicationUser user, string? ipAddress)
        {
            var (accessToken, accessExp) = await _jwtService.GenerateAccessTokenAsync(user, _userManager);

            var refreshTokenRepo = _unitOfWork.Repository<RefreshToken, int>();
            var existingTokens = await refreshTokenRepo.FindAsync(t =>
                t.UserId == user.Id && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
            var existingToken = existingTokens.OrderByDescending(t => t.ExpiresAt).FirstOrDefault();

            string refreshToken;
            DateTime refreshExp;

            if (existingToken != null)
            {
                refreshToken = existingToken.Token;
                refreshExp = existingToken.ExpiresAt;
            }
            else
            {
                (refreshToken, refreshExp) = _jwtService.GenerateRefreshToken();
                await refreshTokenRepo.AddAsync(new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = refreshExp,
                    CreatedByIp = ipAddress
                });
                await _unitOfWork.CompleteAsync();
            }

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExp,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshExp
            };
        }
    }
}
