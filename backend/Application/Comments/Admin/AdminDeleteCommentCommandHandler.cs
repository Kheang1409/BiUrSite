using Backend.Domain.Comments;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Comments.Admin;

internal sealed class AdminDeleteCommentCommandHandler : IRequestHandler<AdminDeleteCommentCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<AdminDeleteCommentCommandHandler> _logger;

    public AdminDeleteCommentCommandHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        ILogger<AdminDeleteCommentCommandHandler> logger)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public async Task Handle(AdminDeleteCommentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin deleting comment {CommentId} on post {PostId} with reason: {Reason}",
            request.CommentId, request.PostId, request.Reason);

        var postId = new PostId(request.PostId);
        var commentId = new CommentId(request.CommentId);

        var post = await _postRepository.GetPostById(postId);
        if (post is null)
            throw new NotFoundException("Post not found.");

        if (post.Status != Status.Active)
            throw new ConflictException("Cannot delete comment on an inactive post.");

        var comment = await _commentRepository.GetCommentById(postId, commentId);
        if (comment is null)
            throw new NotFoundException("Comment not found.");

        if (comment.Status == Status.Deleted)
            throw new ConflictException("Comment is already deleted.");

        comment.Delete();
        await _commentRepository.Delete(postId, comment);

        _logger.LogInformation("Comment {CommentId} on post {PostId} has been deleted by admin. Reason: {Reason}",
            request.CommentId, request.PostId, request.Reason);
    }
}
