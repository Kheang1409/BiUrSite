using Backend.Application.Data;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Posts.Create;

internal sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Post>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostFactory _postFactory;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public CreatePostCommandHandler(
        IPostRepository postRepository,
        IPostFactory postFactory,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _postRepository = postRepository;
        _postFactory = postFactory;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<Post> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = _postFactory.Create(
            new UserId(request.UserId),
            request.Username,
            request.Text,
            request.Data
        );
        await _postRepository.Create(post);
        await _domainEventDispatcher.DispatchAsync(post, cancellationToken);
        return post;
    }
}