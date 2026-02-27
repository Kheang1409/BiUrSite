using Backend.Domain.Comments;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Comments.Delete;

internal sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<DeleteCommentCommandHandler> _logger;

    public DeleteCommentCommandHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        ILogger<DeleteCommentCommandHandler> logger
    )
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }
    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DeleteComment: PostId={PostId}, CommentId={CommentId}, UserId={UserId}", 
            request.PostId, request.Id, request.UserId);
        
        var postId = new PostId(request.PostId);
        var commentId = new CommentId(request.Id);
        var post = await _postRepository.GetPostById(postId);
        
        _logger.LogInformation("DeleteComment: Post found={Found}", post != null);
        
        if (post is null)
            throw new NotFoundException("Post not found.");
        if (post.Status != Status.Active)
            throw new UnauthorizedAccessException($"Post is {post.Status}.");
        if (post.UserId is null)
            throw new InvalidOperationException("Post has no owner.");
            
        var comment = await _commentRepository.GetCommentById(postId, commentId);
        
        _logger.LogInformation("DeleteComment: Comment found={Found}", comment != null);
        
        if (comment is null)
            throw new NotFoundException("Comment not found.");
        if (comment.Status != Status.Active)
            throw new UnauthorizedAccessException($"Comment is {comment.Status}.");
        if (comment.UserId is null)
            throw new InvalidOperationException("Comment has no owner.");
        if (!comment.UserId.Value.Equals(request.UserId) && !post.UserId.Value.Equals(request.UserId))
            throw new ForbiddenException("You are not authorized to delete this comment.");
        comment.Delete();
        await _commentRepository.Delete(postId, comment);
        
        _logger.LogInformation("DeleteComment: Successfully deleted comment {CommentId}", request.Id);
    }
}
