using Backend.DTOs;

namespace Backend.Models{
    public class CommentDto{
        public int commentId {get; set;}
        public required string description {get; set;}
        public UserDto? user {get; set;}
        public int postId {get; set;}
        public DateTime createdDate {get; set;} = DateTime.UtcNow; 
        public DateTime? modifiedDate {get; set;}
    }
}