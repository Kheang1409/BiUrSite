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
        var comment = await _commentRepository.GetCommentById(postId, commentId);
        if (post == null)
            throw new NotFoundException("Post not found.");
        if (comment == null)
            throw new NotFoundException("Comment not found.");
        
        var user = await _userRepository.GetUserById(post.UserId);
        if (user is not null && !user.Id.Value.Equals(comment.UserId.Value))
        {
            var notification = user.AddNotification(
                post.Id,
                comment.Id,
                $"{comment.Username} commented on your post",
                comment.Text
            );
            await Task.WhenAll(_notificationRepository.Create(user.Id, notification), _notificationNotifier.NotifyPostOwnerOfComment(notification));
        }
    }
}
