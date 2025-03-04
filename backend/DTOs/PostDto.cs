using Backend.DTOs;

namespace Backend.Models{
    public class PostDto{
        public int postId {get; set;}
        public required string description {get; set;}
        public int userId {get; set;}
        public UserDto? user {get; set;}
        public DateTime createdDate {get; set;} = DateTime.UtcNow; 
        public DateTime? modifiedDate {get; set;}
        // public List<Comment> comments {get; set;} = new List<Comment>();

    }
}