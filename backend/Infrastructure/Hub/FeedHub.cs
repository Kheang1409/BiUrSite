using Backend.Application.DTOs.Posts;
using Backend.Domain.Posts;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Infrastructure.Hubs;

public class FeedHub : Hub
{
    public async Task BroadcastPost(Post post)
    {
        await Clients.All.SendAsync("ReceivePost", (PostDto) post);
    }
}
