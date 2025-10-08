using Backend.Application.Services;
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

    
    public async Task NotifyPostOwnerOfComment(Guid postOwnerUserId, string commenterUsername, string commentText, string postId)
    {
        await _hubContext.Clients
            .User(postOwnerUserId.ToString())
            .SendAsync("ReceiveCommentNotification", new
            {
                PostId = postId,
                Commenter = commenterUsername,
                Text = commentText
            });
    }
}
