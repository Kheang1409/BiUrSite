using Backend.Application.Data;
using Backend.Domain.Comments;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Comments.Create;

internal sealed class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Comment>
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public CreateCommentCommandHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<Comment> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostById(request.PostId);
        var comment = post.AddComment(
            new UserId(request.UserId),
            request.Username,
            request.Text);
            
        await _commentRepository.Create(request.PostId, comment);
        await _domainEventDispatcher.DispatchAsync(post, cancellationToken);
        return comment;
    }
}