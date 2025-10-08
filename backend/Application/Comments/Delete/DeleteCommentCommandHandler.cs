using Backend.Domain.Comments;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Comments.Delete;

public record DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;

    public DeleteCommentCommandHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository
    )
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
    }
    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var postId = new PostId(request.PostId);
        var commentId = new CommentId(request.Id);
        var post = await _postRepository.GetPostById(postId);
        if (post is null)
            throw new NotFoundException("Post is not found.");
        var comment = await _commentRepository.GetCommentById(postId, commentId);
        if (comment is null)
            throw new NotFoundException("Comment is not found.");
        if (!comment.UserId.Value.Equals(request.UserId))
            throw new ForbiddenException("You are not authorized to delete this comment.");
        post.Delete();
        await _commentRepository.Delete(post.Id, comment);
    }
}
