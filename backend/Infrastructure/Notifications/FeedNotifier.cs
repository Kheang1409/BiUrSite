using Backend.Application.DTOs.Posts;
using Backend.Application.Services;
using Backend.Domain.Posts;
using Backend.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Infrastructure.Notifications;

public class FeedNotifier : IFeedNotifier
{
    private readonly IHubContext<FeedHub> _hubContext;

    public FeedNotifier(IHubContext<FeedHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastPost(Post post)
    {
        await _hubContext.Clients.All.SendAsync("ReceivePost", (PostDto)post);
    }
}
