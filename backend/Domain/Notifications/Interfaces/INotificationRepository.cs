using Backend.Domain.Notifications.Entities;

namespace Backend.Domain.Notifications.Interfaces;
public interface INotificationRepository
{
    Task<Notification> CreateNotificationAsync(Notification notification);
    Task<List<Notification>> GetNotificationsByUserIdAsync(int userId);
    Task<Notification?> GetNotificationByIdAsync(int notificationId);
    Task<bool> UpdateNotificationReadStatusAsync(int notificationId, bool isRead);
    Task<bool> SoftDeleteNotificationAsync(int notificationId);
    Task<bool> DeleteNotificationAsync(int notificationId);
}
