using Backend.Domain.Notifications;
using Backend.Domain.Users;

namespace Backend.Application.Services;

public interface INotificationNotifier
{
    Task NotifyPostOwnerOfComment(UserId userId, Notification notification);
}
