using Backend.DTOs;

namespace Backend.Models{
    public class CommentDto{
        public int commentId {get; set;}
        public required string description {get; set;}
        public bool isDeleted {get; set;}
        public DateTime? createdDate {get; set;}
        public DateTime? modifiedDate {get; set;}
        public UserDto? commenter {get; set;}
    }
}