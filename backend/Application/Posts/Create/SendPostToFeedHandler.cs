using Backend.Application.Services;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using Rebus.Handlers;

namespace Backend.Application.Posts.Create;

internal sealed class SendPostToFeedHandler : IHandleMessages<PostCreatedEvent>
{
    private readonly IPostRepository _postRepository;
    private readonly IFeedNotifier _feedNotifier;
    public SendPostToFeedHandler(
        IPostRepository postRepository,
        IFeedNotifier feedNotifier)
    {
        _postRepository = postRepository;
        _feedNotifier = feedNotifier;
    }

    public async Task Handle(PostCreatedEvent message)
    {
        var post = await _postRepository.GetPostById(new PostId(message.Id));
        if (post == null)
            throw new NotFoundException("Post not found.");
        await _feedNotifier.BroadcastPost(post);
    }
}
