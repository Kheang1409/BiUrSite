using Backend.Models;

namespace Backend.Services{
    public interface INotificationService
    {
        // Email notification
        Task SendOtpEmail(string recipientEmail, string? otp);
        Task SendConfirmationEmail(string recipientEmail, string confirmationLink);
        // Email notifications
        Task<Notification> AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetUnreadNotificationsAsync(int userId);
        Task<Notification?> GetNotificationByIdAsync(int notificationId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<bool> DeleteNotificationAsync(int notificationId);
    }
}