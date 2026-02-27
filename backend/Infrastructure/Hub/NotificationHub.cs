using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public async Task NotifyUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveCommentNotification", message);
    }

    public override Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier ?? "(anonymous)";
        _logger.LogDebug("NotificationHub connected. ConnectionId={ConnectionId} User={UserId}", Context.ConnectionId, userId);

        try
        {
            var claims = Context.User?.Claims?.Select(c => new { c.Type, c.Value })?.ToList();
            _logger.LogDebug("Connection claims for ConnectionId={ConnectionId}: {Claims}", Context.ConnectionId, JsonSerializer.Serialize(claims));
        }
        catch { }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier ?? "(anonymous)";
        _logger.LogDebug("NotificationHub disconnected. ConnectionId={ConnectionId} User={UserId} Exception={Exception}", Context.ConnectionId, userId, exception?.Message);
        return base.OnDisconnectedAsync(exception);
    }
}