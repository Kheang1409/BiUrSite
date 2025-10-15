using Backend.Application.DTOs.Notifications;
using Backend.Application.Services;
using Backend.Domain.Notifications;
using Backend.Domain.Users;
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

    
    public async Task NotifyPostOwnerOfComment(UserId userId, Notification notification)
    {
        await _hubContext.Clients
            .User(userId.Value.ToString())
            .SendAsync("ReceiveCommentNotification", (NotificationDTO) notification);
    }
}
