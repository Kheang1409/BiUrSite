using Backend.DTOs;

namespace Backend.Models{
    public class PostDto{
        public int postId {get; set;}
        public required string description {get; set;}
        public bool isDeleted {get; set;}
        public DateTime? createdDate {get; set;} 
        public DateTime? modifiedDate {get; set;}
        public UserDto? author {get; set;}
    }
}