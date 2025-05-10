using Backend.Domain.Notifications.Entities;
using Backend.Domain.Notifications.Interfaces;
using MediatR;

namespace Backend.Application.Features.Notifications.CreateNotification;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, int>
{
    private readonly INotificationRepository _notificationRepository;

    public CreateNotificationCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<int> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = Notification.Create(
            request.UserId,
            request.Message,
            request.PostId,
            request.CommentId
        );
        await _notificationRepository.CreateNotificationAsync(notification);

        return notification.NotificationId;
    }
}
