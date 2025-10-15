using Microsoft.AspNetCore.SignalR;

namespace Backend.Infrastructure.Hubs;

public class NotificationHub : Hub
{
    public async Task NotifyUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveCommentNotification", message);
    }
}