using System.Text.Json.Serialization;
using Backend.Domain.Posts.Entities;
using Backend.Domain.Users.Entities;

namespace Backend.Domain.Comments.Entities;
public class Comment{
    public int Id {get; set;}
    public string Description {get; set;}
    public int UserId {get; set;}
    public int PostId {get; set;}
    public bool IsDeleted {get; set; }
    public DateTime CreatedDate {get; set;}
    public DateTime? ModifiedDate {get; set;}
    public DateTime? DeletedDate { get; set; }
    [JsonIgnore]
    public User? Commenter {get; set;}
    public Post? Post {get; set;}

    private Comment(int userId, int postId, string description){
        UserId = userId;
        PostId = postId;
        Description = description;
        CreatedDate = DateTime.UtcNow;
        IsDeleted = false;
    }
    public static Comment Create(int userId, int postId, string description){
        return new Comment(userId, postId, description);
    }
    public void Deleted(){
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
    }
    public void UpdateDescription(string newDescription){
        Description = newDescription;
        ModifiedDate = DateTime.UtcNow;
    }
}