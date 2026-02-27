using Backend.Domain.Posts;

namespace Backend.Application.Services;

public interface IFeedNotifier
{
    Task BroadcastPost(Post post);
}
