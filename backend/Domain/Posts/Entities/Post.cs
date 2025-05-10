using Backend.Domain.Comments.Entities;
using Backend.Domain.Users.Entities;

namespace Backend.Domain.Posts.Entities;
public class Post
{
    public int PostId { get; private set; }
    public string Description { get; private set; }
    public bool IsDeleted { get; private set; }
    public int UserId { get; private set; }
    public DateTime CreatedDate { get; }
    public DateTime? ModifiedDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public User? Author { get; private set; }
    public List<Comment> Comments { get; } = new List<Comment>();

    private Post(int userId, string description)
    {
        UserId = userId;
        Description = description;
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }

    public static Post Create(int userId, string description)
    {
        return new Post(userId, description);
    }
}