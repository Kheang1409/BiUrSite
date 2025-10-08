using Backend.Application.Services;
using Backend.Domain.Comments;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using Rebus.Handlers;

namespace Backend.Application.Comments.Create;

internal sealed class SendNotificationPostOwnerHandler : IHandleMessages<CommentCreatedEvent>
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly INotificationNotifier _notificationNotifier;
    public SendNotificationPostOwnerHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        INotificationNotifier notificationNotifier)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _notificationNotifier = notificationNotifier;
    }

    public async Task Handle(CommentCreatedEvent message)
    {
        var postId = new PostId(message.PostId);
        var commentId = new CommentId(message.Id);
        
        var post = await _postRepository.GetPostById(postId);
        var comment = await _commentRepository.GetCommentById(postId, commentId);
        if (post == null)
            throw new NotFoundException("Post not found.");
        if (comment == null)
            throw new NotFoundException("Comment not found.");
        if (!post.Id.Value.Equals(comment.UserId.Value))
        {
            await _notificationNotifier.NotifyPostOwnerOfComment(
                message.UserId,
                comment.Username,
                comment.Text,
                message.PostId
            );
        }
    }
}
