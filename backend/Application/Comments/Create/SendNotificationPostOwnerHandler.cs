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
        var post = await _postRepository.GetPostById(message.PostId);
        var comment = await _commentRepository.GetCommentById(message.PostId, message.Id);
        if (post == null)
            throw new NotFoundException("Post not found.");
        if (comment == null)
            throw new NotFoundException("Comment not found.");
        if (!post.UserIdValue.Equals(comment.UserIdValue))
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
