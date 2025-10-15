using Backend.Domain.Notifications;

namespace Backend.Application.Services;

public interface INotificationNotifier
{
    Task NotifyPostOwnerOfComment(Notification notification);
}
