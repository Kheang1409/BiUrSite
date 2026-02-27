using Backend.Domain.Notifications;

namespace Backend.Application.DTOs.Notifications;

public record NotificationDTO(
    string Id,
    string PostId,
    string UserId,
    string Username,
    string UserProfile,
    string Title,
    string Message,
    DateTime CreatedDate
)
{
    public static explicit operator NotificationDTO(Notification notification)
    {
        return new NotificationDTO(
            notification.Id.Value.ToString(),
            notification.PostId.Value.ToString(),
            notification.UserId.Value.ToString(),
            notification.User!.Username,
            notification.User!.Profile.Url,
            notification.Title,
            notification.Message,
            notification.CreatedDate
        );
    }
}