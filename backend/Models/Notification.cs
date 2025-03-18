using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models{
    public class Notification {
        [Key]
        public int notificationId {get; set;}
        [Required(ErrorMessage = "Message is required.")]
        public required string message {get; set;}
        [Required(ErrorMessage = "UserId is required.")]
        public int userId {get; set;}

        [Required(ErrorMessage = "PostId is required.")]
        public int postId {get; set;}
        [Required(ErrorMessage = "CommentId is required.")]
        public int commentId {get; set;}
        public DateTime? createdDate {get; set;}
        public bool isRead {get; set;}
        [ForeignKey("postId")]
        [JsonIgnore]
        public Post post {get; set;} 
        [ForeignKey("commentId")]
        [JsonIgnore]
        public Comment comment {get; set;} 
    }
}