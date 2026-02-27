using Backend.Domain.Enums;
using Backend.Domain.Primitive;
using Backend.Domain.Users;

namespace Backend.Domain.Comments;

public class Comment : Entity
{
    public CommentId Id { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public User? User { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public Status Status { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }


    private Comment(){ }
    private Comment(User user, string text)
    {
    Id = new CommentId(Guid.NewGuid());
    UserId = user.Id;
    User = user;
    Text = text;
    Status = Status.Active;
    CreatedDate = DateTime.UtcNow;
    }

    internal static Comment Create(User user, string text)
    {
        var comment = new Comment(user, text);
        return comment;
    }
    public void UpdateContent(string text)
    {
        Text = text;
        ModifiedDate = DateTime.UtcNow;
    }

    public void SetUser(User user)
    {
        User = user;
    }

    public void Delete()
    {
        Status = Status.Deleted;
        DeletedDate = DateTime.UtcNow;
    }

}