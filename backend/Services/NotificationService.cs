using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Notification> AddNotificationAsync(Notification notification)
        {
            return await _notificationRepository.AddNotificationAsync(notification);
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(int userId)
        {
            return await _notificationRepository.GetUnreadNotificationsAsync(userId);
        }

        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
        {
            return await _notificationRepository.GetNotificationByIdAsync(notificationId);
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            return await _notificationRepository.MarkNotificationAsReadAsync(notificationId);
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            return await _notificationRepository.DeleteNotificationAsync(notificationId);
        }
    }
}