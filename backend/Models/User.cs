using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Backend.Enums;

namespace Backend.Models{
    public class User{
        [Key]
        public int userId {get; set;}
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(maximumLength: 20, MinimumLength = 5, ErrorMessage = "Username must be between 5 and 15 characters.")]
        public required string username {get; set;}
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string email {get; set;}
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(maximumLength: 150, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 15 characters long.")]
        public required string password {get; set;}
        [Required]
        public bool isActive {get; set;} = false;
        [Required]
        public Status status {get; set;} = Status.Unverified;
        [Required]
        public Role role {get; set; } = Role.User;
        public string? opt { get; set; }
        public DateTime? optExpiry { get; set; }
        public string? verificationToken  { get; set; }
        public DateTime? verificationTokenExpiry  { get; set; }
        public DateTime? createdDate {get; set;} = DateTime.UtcNow;
        public DateTime? modifiedDate {get; set;}
        [JsonIgnore]
        public List<Post>? posts {get; set;} = new List<Post>();
        public List<Comment>? comments {get; set;} = new List<Comment>();
    }
}