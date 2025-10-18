using Backend.Domain.Primitive;
using Backend.Domain.Users;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Domain.Notifications;

public class Notification : Entity
{
    private const string DEFAULT_TITLLE = "commented on your post";
    public NotificationId Id { get; private set; } = default!;
    public UserId UserId { get; private set; } = default!;
    public PostId PostId { get; private set; } = default!;
    [BsonIgnore]
    public User? User { get; private set; }
    public string Title { get; private set; } = DEFAULT_TITLLE;
    public string Message { get; private set; } = string.Empty;
    public Status Status { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }

    private Notification() { }

    private Notification(UserId userId, PostId postId, string message)
    {
        Id = new NotificationId(Guid.NewGuid());
        UserId = userId;
        PostId = postId;
        Message = message;
        Status = Status.Active;
        CreatedDate = DateTime.UtcNow;
    }

    public void SetUser(User user){
        User = user;
    }

    public static Notification Create(UserId userId, PostId postId, string message)
    {
        if (userId is null)
            throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrWhiteSpace(message))
            throw new InvalidOperationException("Message cannot be empty.");
        var notification = new Notification(userId, postId, message);
        return notification;
    }
}