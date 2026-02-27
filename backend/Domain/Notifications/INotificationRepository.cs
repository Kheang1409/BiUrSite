using Backend.Domain.Users;

namespace Backend.Domain.Notifications;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetNotifications(UserId userId, int pageNumber);
    Task<Notification> Create(UserId userId, Notification notification);
    Task Delete(UserId userId, Notification notification);
}