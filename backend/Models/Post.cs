using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models{
    public class Post{
        [Key]
        public int postId {get; set;}
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(maximumLength: 3000, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 3000 characters.")]
        public required string description {get; set;}
        [Required(ErrorMessage = "UserId is required.")]
        public int userId {get; set;}
        [ForeignKey("userId")]
        [JsonIgnore]
        public User? user {get; set;}
        public DateTime createdDate {get; set;} = DateTime.UtcNow; 
        public DateTime? modifiedDate {get; set;}
        // public List<Comment> comments {get; set;} = new List<Comment>();

    }
}