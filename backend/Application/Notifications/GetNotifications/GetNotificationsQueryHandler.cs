using Backend.Domain.Notifications;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Notifications.GetNotifications;

internal sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, IEnumerable<Notification>>
{
    private readonly INotificationRepository _notificationRepository;
    public GetNotificationsQueryHandler(
        INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<Notification>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        return await _notificationRepository.GetNotifications(new UserId(request.UserId), request.PageNumber);
    }
}