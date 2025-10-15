using Backend.Application.Services;
using Backend.Domain.Notifications;
using Backend.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Infrastructure.Notifications;

public class NotificationNotifier : INotificationNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    
    public async Task NotifyPostOwnerOfComment(Notification notification)
    {
        await _hubContext.Clients
            .User(notification.RecipientId.Value.ToString())
            .SendAsync("ReceiveCommentNotification", notification);
    }
}
