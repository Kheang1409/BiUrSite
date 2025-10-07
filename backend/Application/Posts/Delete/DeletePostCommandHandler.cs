using Backend.Application.Data;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Posts.Delete;

public record DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;


    public DeletePostCommandHandler(
        IPostRepository postRepository,
        IDomainEventDispatcher domainEventDispatcher
    )
    {
        _postRepository = postRepository;
        _domainEventDispatcher = domainEventDispatcher;
    }
    public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostById(request.Id);
        if (post is null)
            throw new NotFoundException("Post is not found.");
        if (!post.UserId.Value.Equals(request.UserId))
            throw new ForbiddenException("You are not authorized to edit this post.");
        post.Delete();
        await _postRepository.Delete(post);
        await _domainEventDispatcher.DispatchAsync(post, cancellationToken);
    }
}
