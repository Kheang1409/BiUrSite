using Backend.Domain.Enums;
using Backend.Domain.Primitive;
using Backend.Domain.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Domain.Comments;

public class Comment : Entity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; private set; }
    [BsonIgnore]
    public UserId UserId { get; private set; }
    [BsonElement("UserId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserIdValue
    {
        get => UserId.Value;
        private set => UserId = new UserId(value);
    }
    public string Username { get; private set; }
    public string Text { get; private set; }
    public Status Status { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }


    private Comment() { }
    private Comment(UserId userId, string username, string text)
    {
        Id = ObjectId.GenerateNewId().ToString();
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