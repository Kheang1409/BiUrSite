using Backend.Domain.Notifications;
using Backend.Domain.Posts;
using MediatR;

namespace Backend.Application.Notifications.GetNotifications;

public record GetNotificationsQuery(
    Guid UserId,
    int PageNumber=1) : IRequest<IEnumerable<Notification>>;
