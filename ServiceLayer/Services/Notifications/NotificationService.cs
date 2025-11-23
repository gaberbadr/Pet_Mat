using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Dtos.Notification;
using CoreLayer.Entities;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Notifications;
using CoreLayer.Service_Interface.Notification;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using petmat.Hubs;

namespace ServiceLayer.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IHubContext<ChatHub> hubContext,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        // ==================== USER OPERATIONS ====================

        public async Task<NotificationListDto> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _unitOfWork.Repository<Notification, int>()
                .FindAsync(n => n.UserId == userId);

            var orderedNotifications = notifications
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToList();

            var unreadCount = notifications.Count(n => !n.IsRead);

            return new NotificationListDto
            {
                UnreadCount = unreadCount,
                Data = orderedNotifications
            };
        }

        // Get count of unread notifications
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var notifications = await _unitOfWork.Repository<Notification, int>()
                .FindAsync(n => n.UserId == userId && !n.IsRead);

            return notifications.Count();
        }

        // Mark all notifications as read for a user
        public async Task MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _unitOfWork.Repository<Notification, int>()
                .FindAsync(n => n.UserId == userId && !n.IsRead);

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                _unitOfWork.Repository<Notification, int>().Update(notification);
            }

            await _unitOfWork.CompleteAsync();

            // Update unread count via SignalR
            await SendUnreadCountUpdate(userId);
        }

        // ==================== SYSTEM/ADMIN OPERATIONS ====================

        // Add a new notification for a user
        public async Task AddNotificationAsync(string userId, string message)
        {
            // Check if user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{userId}' not found");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                throw new InvalidOperationException($"User with ID '{userId}' is not active");
            }

            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _unitOfWork.Repository<Notification, int>().AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                // Send ONLY unread count update via SignalR (no notification content)
                await SendUnreadCountUpdate(userId);
            }
            catch (Exception ex)
            {
                // Log the detailed error
                Console.WriteLine($"Error saving notification: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw new InvalidOperationException("Failed to save notification to database", ex);
            }
        }

        // ==================== HELPER METHODS ====================

        private async Task SendUnreadCountUpdate(string userId)
        {
            try
            {
                var unreadCount = await GetUnreadCountAsync(userId);
                await _hubContext.Clients.User(userId)
                    .SendAsync("UnreadNotificationsCount", unreadCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending unread count: {ex.Message}");
            }
        }
    }
}
