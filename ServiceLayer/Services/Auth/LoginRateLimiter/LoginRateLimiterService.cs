using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Entities.Identity;
using CoreLayer.Service_Interface;

namespace ServiceLayer.Services.Auth.LoginRateLimiter
{//He is allowed to make a maximum of 5 failed attempts within the last 15 minutes. If he exceeds this limit, he is banned from making further attempts for the next 3 hours.
    public class LoginRateLimiterService : ILoginRateLimiterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _maxAttempts = 5;
        private readonly TimeSpan _banDuration = TimeSpan.FromHours(3);
        private readonly TimeSpan _attemptWindow = TimeSpan.FromMinutes(15);//Any failed attempts made within the past 15 minutes will not count toward the maximum limit (_maxAttempts = 5)., Only attempts within the last 15 minutes will count.

        public LoginRateLimiterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool IsAllowed, TimeSpan? BanDuration)> CheckLoginAttemptAsync(string email)
        {
            var attemptsRepo = _unitOfWork.Repository<LoginAttempt, int>();
            var cutoffTime = DateTime.UtcNow.Subtract(_attemptWindow);

            // Get recent failed attempts
            var recentFailedAttempts = await attemptsRepo.FindAsync(a =>
                a.Email == email &&
                !a.IsSuccessful &&
                a.AttemptedAt > cutoffTime);

            var failedCount = recentFailedAttempts.Count();

            if (failedCount >= _maxAttempts)//user exceeded max attempts
            {
                var oldestFailedAttempt = recentFailedAttempts.OrderBy(a => a.AttemptedAt).First();
                var banExpiresAt = oldestFailedAttempt.AttemptedAt.Add(_banDuration);
                var remainingBanTime = banExpiresAt - DateTime.UtcNow;

                if (remainingBanTime > TimeSpan.Zero)
                {
                    return (false, remainingBanTime);
                }
            }

            return (true, null);
        }

        public async Task RecordLoginAttemptAsync(string email, bool isSuccessful, string ipAddress)
        {
            var attemptsRepo = _unitOfWork.Repository<LoginAttempt, int>();

            await attemptsRepo.AddAsync(new LoginAttempt
            {
                Email = email,
                IsSuccessful = isSuccessful,
                IpAddress = ipAddress,
                AttemptedAt = DateTime.UtcNow
            });

            await _unitOfWork.CompleteAsync();

            // Clean up old attempts (older than 24 hours)
            var cleanupTime = DateTime.UtcNow.AddHours(-24);
            await attemptsRepo.DeleteRangeAsync(a => a.AttemptedAt < cleanupTime);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ResetLoginAttemptsAsync(string email)
        {
            var attemptsRepo = _unitOfWork.Repository<LoginAttempt, int>();
            await attemptsRepo.DeleteRangeAsync(a => a.Email == email);
            await _unitOfWork.CompleteAsync();
        }
    }
}
