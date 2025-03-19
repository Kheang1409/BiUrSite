using Backend.Models;

namespace Backend.Services{
    public interface INotificationService
    {
        Task<Notification> AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetUnreadNotificationsAsync(int userId);
        Task<Notification?> GetNotificationByIdAsync(int notificationId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<bool> DeleteNotificationAsync(int notificationId);
    }
}