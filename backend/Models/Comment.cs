using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models{
    public class Comment{
        [Key]
        public int commentId {get; set;}
        [Required(ErrorMessage = "description is required.")]
        public required string description {get; set;}
        [Required(ErrorMessage = "UserId is required.")]
        public bool isDeleted {get; set; }
        [Required(ErrorMessage = "UserId is required.")]
        public int userId {get; set;}
        [Required(ErrorMessage = "PostId is required.")]
        public int postId {get; set;}
        public DateTime? createdDate {get; set;}
        public DateTime? modifiedDate {get; set;}
        public DateTime? deletedDate { get; set; }
        [ForeignKey("userId")]
        [JsonIgnore]
        public User? commenter {get; set;}
        [ForeignKey("postId")]
        [JsonIgnore]
        public Post? post {get; set;}
    }
}