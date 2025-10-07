using Backend.Domain.Enums;
using Backend.Domain.Primitive;
using Backend.Domain.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Domain.Posts;

public class Post : Entity
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
    public Image Image { get; private set; }
    public Status Status { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    private Post() { }

    private Post(Builder builder)
    {
        Id = ObjectId.GenerateNewId().ToString();
        UserId = builder.UserId;
        Username = builder.Username;
        Text = builder.Text;
        Status =  Status.Active;
        CreatedDate = DateTime.UtcNow;
    }

    public class Builder
    {
        internal UserId UserId;
        internal string Username = string.Empty;
        internal string Text = string.Empty;
        internal byte[]? Data;

        public Builder WithUserId(UserId userId)
        {
            UserId = userId;
            return this;
        }

        public Builder WithUsername(string username)
        {
            Username = username;
            return this;
        }

        public Builder WithText(string content)
        {
            Text = content;
            return this;
        }

        public Builder WithImage(byte[]? data)
        {
            Data = data;
            return this;
        }

        public Post Build()
        {
            if (UserId == null || UserId.Value == Guid.Empty)
                throw new ArgumentException("UserId must be provided and not empty.", nameof(UserId));

            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username must be provided.", nameof(Username));

            if (string.IsNullOrWhiteSpace(Text))
                throw new ArgumentException("Text must be provided.", nameof(Text));

            var post = new Post(this);
            post.AddDomainEvent(new PostCreatedDomainEvent(Guid.NewGuid(), post.Id, Data));
            return post;
        }
    }

    public void SetImage(string id, string url)
    {
        Image = new Image(id, url);
        ModifiedDate = DateTime.UtcNow;
    }

    public void Delete()
    {
        Status = Status.Deleted;
        DeletedDate = DateTime.UtcNow;
        AddDomainEvent(new PostDeletedDomainEvent(Guid.NewGuid(), this.Id, this.Image));
    }

    public void UpdateContent(string content)
    {
        Text = content;
        ModifiedDate = DateTime.UtcNow;
    }

}