namespace Backend.Application.Services;

public interface INotificationNotifier
{
    Task NotifyPostOwnerOfComment(
        Guid postOwnerUserId,
        string commenterUsername,
        string commentText,
        Guid postId);
}
