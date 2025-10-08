using Backend.Domain.Enums;
using Backend.Domain.Primitive;
using Backend.Domain.Users;

namespace Backend.Domain.Comments;

public class Comment : Entity
{
    public CommentId Id { get; private set; }
    public UserId UserId { get; private set; }
    public string Username { get; private set; }
    public string Text { get; private set; }
    public Status Status { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }


    private Comment() { }
    private Comment(UserId userId, string username, string text)
    {
        Id = new CommentId(Guid.NewGuid());
        UserId = userId;
        Username = username;
        Text = text;
        Status = Status.Active;
        CreatedDate = DateTime.UtcNow;
    }

    internal static Comment Create(UserId userId, string username, string text)
    {
        var comment = new Comment(userId, username, text);
        return comment;
    }
    public void UpdateContent(string text)
    {
        Text = text;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Delete()
    {
        Status = Status.Deleted;
        DeletedDate = DateTime.UtcNow;
    }

}