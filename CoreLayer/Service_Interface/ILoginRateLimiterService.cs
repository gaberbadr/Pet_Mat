using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Service_Interface
{
    public interface ILoginRateLimiterService
    {
        Task<(bool IsAllowed, TimeSpan? BanDuration)> CheckLoginAttemptAsync(string email);
        Task RecordLoginAttemptAsync(string email, bool isSuccessful, string ipAddress);
        Task ResetLoginAttemptsAsync(string email);
    }
}
