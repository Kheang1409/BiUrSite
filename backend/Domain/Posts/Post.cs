using Backend.Domain.Comments;
using Backend.Domain.Enums;
using Backend.Domain.Images;
using Backend.Domain.Primitive;
using Backend.Domain.Users;

namespace Backend.Domain.Posts;

public class Post : Entity
{
    public PostId Id { get; private set; }
    public UserId UserId { get; private set; }
    public User? User { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public Image? Image { get; private set; }
    private List<Comment> _comments = new();
    public IEnumerable<Comment> Comments => _comments;
    public Status Status { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    private Post()
    {
        Id = new PostId(Guid.Empty);
        UserId = new UserId(Guid.Empty);
    }

    private Post(Builder builder)
    {
        Id = new PostId(Guid.NewGuid());
        UserId = builder.UserId ?? throw new ArgumentNullException(nameof(builder.UserId));
        Text = builder.Text;
        Status = Status.Active;
        CreatedDate = DateTime.UtcNow;
        User = builder.User;
    }

    public class Builder
    {
    internal UserId? UserId;
    internal User? User;
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

        public Builder WithUser(User user)
        {
            User = user;
            return this;
        }

        public Post Build()
        {
            if (UserId == null || UserId.Value == Guid.Empty)
                throw new ArgumentException("UserId must be provided and not empty.", nameof(UserId));

            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("Username must be provided.", nameof(Username));

            var hasText = !string.IsNullOrWhiteSpace(Text);
            var hasImage = Data is { Length: > 0 };

            if (!hasText && !hasImage)
                throw new ArgumentException("Post must contain text or image.");

            var post = new Post(this);
            post.User = User;
            post.AddDomainEvent(new PostCreatedDomainEvent(Guid.NewGuid(), post.Id!, Data));
            return post;
        }
    }

    public void SetImage(string url)
    {
        Image = new Image(url);
        ModifiedDate = DateTime.UtcNow;
        AddDomainEvent(new PostUpdatedDomainEvent(Guid.NewGuid(), Id, UserId, null, url));
    }

    public void RemoveImage()
    {
        Image = null;
        ModifiedDate = DateTime.UtcNow;
        AddDomainEvent(new PostUpdatedDomainEvent(Guid.NewGuid(), Id, UserId, null, null));
    }

    public void Delete()
    {
        if (Status == Status.Deleted)
            return; // Idempotent - already deleted
            
        Status = Status.Deleted;
        DeletedDate = DateTime.UtcNow;
        AddDomainEvent(new PostDeletedDomainEvent(Guid.NewGuid(), Id!, Image));
    }

    public void UpdateContent(string content)
    {
        Text = content;
        ModifiedDate = DateTime.UtcNow;
        AddDomainEvent(new PostUpdatedDomainEvent(Guid.NewGuid(), Id, UserId, content, Image?.Url));
    }


    public Comment AddComment(User user, string text)
    {
        var comment = Comment.Create(user, text);
        _comments.Add(comment);
        this.AddDomainEvent(new CommentAddedDomainEvent(Guid.NewGuid(), this.Id!, comment.Id!, user.Id));
        return comment;
    }

    public void SetUser(User user)
    {
        User = user;
    }

}