using Backend.Domain.Primitive;
using Backend.Domain.Users;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.Domain.Comments;

namespace Backend.Domain.Notifications;

public class Notification : Entity
{
    private const string BASE_PROFILE_URL = "https://github.com/Kheang1409/images/blob/main/profiles/";
    private const string DEFAULT_TITLLE = "commented on your post";
    public NotificationId Id { get; private set; } = default!;
    public UserId UserId { get; private set; } = default!;
    public PostId PostId { get; private set; } = default!;
    public string Username { get; private set; }
    public string UserProfile { get; private set; }
    public string Title { get; private set; } = DEFAULT_TITLLE;
    public string Message { get; private set; } = string.Empty;
    public Status Status { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }

    private Notification() { }

    private Notification(UserId userId, string username, string userProfile, PostId postId, string message)
    {
        Id = new NotificationId(Guid.NewGuid());
        UserId = userId;
        Username = username;
        UserProfile = $"{BASE_PROFILE_URL}{UserId.Value}.jpg?raw=true";
        PostId = postId;
        Message = message;
        Status = Status.Active;
        CreatedDate = DateTime.UtcNow;
    }

    internal static Notification Create(UserId userId, string username, string userProfile, PostId postId, string message)
    {
        if (userId is null)
            throw new ArgumentNullException(nameof(userId));
        if (username is null)
            throw new ArgumentNullException(nameof(username));
        if (userProfile is null)
            throw new ArgumentNullException(nameof(userProfile));
        if (string.IsNullOrWhiteSpace(message))
            throw new InvalidOperationException("Message cannot be empty.");
        var notification = new Notification(userId, username, userProfile,  postId, message);
        return notification;
    }
}