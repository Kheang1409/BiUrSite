using System.Security.Claims;
using Backend.Models;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task BroadcastPost(PostDto post)
        {
            await Clients.All.SendAsync("ReceivePost", post);
        }

        public async Task NotifyUser(string userId, string message)
        {
            try
            {
                await Clients.User(userId).SendAsync("ReceiveNotification", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
        }
        public async Task NotifyAll(string message)
            => await Clients.All.SendAsync("ReceiveNotification", message);

        public async Task AddToGroup(string groupName)
            => await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        public async Task RemoveFromGroup(string groupName)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        public async Task NotifyGroup(string groupName, string message)
            => await Clients.Group(groupName).SendAsync("ReceiveNotification", message);

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}