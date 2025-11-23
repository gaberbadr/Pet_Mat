using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Notification;

namespace CoreLayer.Service_Interface.Notification
{
    public interface INotificationService
    {
        // User operations
        Task<NotificationListDto> GetUserNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAllAsReadAsync(string userId);

        // System/Admin operations - just add notification to database
        Task AddNotificationAsync(string userId, string message);
    }
}
