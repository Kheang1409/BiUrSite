using Backend.Application.DTOs.Notifications;
using Backend.Application.Services;
using Backend.Domain.Notifications;
using Backend.Domain.Users;
using Backend.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Notifications;

public class NotificationNotifier : INotificationNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationNotifier> _logger;

    public NotificationNotifier(IHubContext<NotificationHub> hubContext, ILogger<NotificationNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyPostOwnerOfComment(UserId userId, Notification notification)
    {
        var target = userId.Value.ToString();
        var actorId = notification.UserId?.Value.ToString() ?? "(unknown)";
        var actorName = notification.User?.Username ?? "(unknown)";
        _logger.LogDebug("Sending notification to user {Target} from actor {ActorId} ({ActorName})", target, actorId, actorName);

        await _hubContext.Clients.User(target).SendAsync("ReceiveCommentNotification", (NotificationDTO)notification);
    }
}
