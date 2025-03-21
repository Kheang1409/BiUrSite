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

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(int userId, bool? isRead)
        {
            return await _notificationRepository.GetNotificationsByUserIdAsync(userId, isRead);
        }

        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
        {
            return await _notificationRepository.GetNotificationByIdAsync(notificationId);
        }

        public async Task<bool> MarkNotificationsync(int notificationId, bool isRead)
        {
            return await _notificationRepository.MarkNotificationsync(notificationId, isRead);
        }

        public async Task<bool> SoftDeleteNotificationAsync(int notificationId){
            return await _notificationRepository.SoftDeleteNotificationAsync(notificationId);
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            return await _notificationRepository.DeleteNotificationAsync(notificationId);
        }
    }
}