using Backend.Domain.Primitive;
using Backend.Domain.Users;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.Domain.Comments;

namespace Backend.Domain.Notifications;

public class Notification : Entity
{
    public NotificationId Id { get; private set; } = default!;
    public UserId RecipientId { get; private set; } = default!;
    public PostId PostId { get; private set; } = default!;
    public CommentId CommentId { get; private set; } = default!;
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public Status Status { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ReadDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }

    private Notification() { }

    private Notification(UserId recipientId, PostId postId, CommentId commentId, string title, string message)
    {
        Id = new NotificationId(Guid.NewGuid());
        RecipientId = recipientId;
        PostId = postId;
        CommentId = commentId;
        Title = title;
        Message = message;
        IsRead = false;
        Status = Status.Active;
        CreatedDate = DateTime.UtcNow;
    }

    internal static Notification Create(UserId recipientId, PostId postId, CommentId commentId, string title, string message)
    {
        if (recipientId is null)
            throw new ArgumentNullException(nameof(recipientId));
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException("Title cannot be empty.");
        if (string.IsNullOrWhiteSpace(message))
            throw new InvalidOperationException("Message cannot be empty.");
        var notification = new Notification(recipientId, postId, commentId, title, message);
        return notification;
    }
}