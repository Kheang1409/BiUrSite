using Backend.Models;

namespace Backend.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetNotificationsByUserIdAsync(int userId, bool? isRead);
        Task<Notification?> GetNotificationByIdAsync(int notificationId);
        Task<bool> MarkNotificationsync(int notificationId, bool isRead);
        Task<bool> SoftDeleteNotificationAsync(int notificationId);
        Task<bool> DeleteNotificationAsync(int notificationId);
    }
}
