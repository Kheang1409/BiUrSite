using Backend.Application.Services;
using Backend.Domain.Comments;
using Backend.Domain.Notifications;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using Rebus.Handlers;

namespace Backend.Application.Comments.Create;

internal sealed class SendNotificationPostOwnerHandler : IHandleMessages<CommentCreatedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationNotifier _notificationNotifier;
    public SendNotificationPostOwnerHandler(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        INotificationRepository notificationRepository,
        INotificationNotifier notificationNotifier)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _notificationRepository = notificationRepository;
        _notificationNotifier = notificationNotifier;
    }

    public async Task Handle(CommentCreatedEvent message)
    {
        var postId = new PostId(message.PostId);
        var commentId = new CommentId(message.Id);
        var post = await _postRepository.GetPostById(postId);
        if (post == null)
            return;
        var comment = await _commentRepository.GetCommentById(postId, commentId);
        if (comment == null)
            return; 

        if (!post.UserId.Value.Equals(comment.UserId.Value))
        {
            var notification = Notification.Create(comment.User!.Id, post.Id, comment.Text);
            notification.SetUser(comment.User);
            post.User!.AddNotification(notification);
            await Task.WhenAll(
                _notificationRepository.Create(post.User.Id, notification),
                _notificationNotifier.NotifyPostOwnerOfComment(post.User.Id, notification)
            );
        }
    }
}
