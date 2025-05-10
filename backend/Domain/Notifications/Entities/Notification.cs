namespace Backend.Domain.Notifications.Entities;

public class Notification
{
    public int NotificationId { get; private set; }
    public string Message { get; private set; }
    public int UserId { get; private set; }
    public int PostId { get; private set; }
    public int CommentId { get; private set; }
    public DateTime? CreatedDate { get; private set; }
    public bool IsRead { get; private set; }
    public bool IsDeleted { get; private set; }

    private Notification(int userId, string message, int postId, int commentId)
    {
        UserId = userId;
        Message = message;
        PostId = postId;
        CommentId = commentId;
        CreatedDate = DateTime.UtcNow;
        IsRead = false;
        IsDeleted = false;
    }

    public static Notification Create(int userId, string message, int postId, int commentId)
    {
        return new Notification(userId, message, postId, commentId);
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}
