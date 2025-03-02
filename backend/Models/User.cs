using System.ComponentModel.DataAnnotations;
namespace Backend.Models{
    public class User{
        [Key]
        public int userId {get; set;}
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(maximumLength: 15, MinimumLength = 5, ErrorMessage = "Username must be between 5 and 15 characters.")]
        public required string username {get; set;}
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string email {get; set;}
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public required string password {get; set;}
        [Required]
        public bool isActive {get; set;} = false;
        [Required]
        public string status {get; set;} = "Unverified";
        [Required]
        public string role {get; set; } = "User";
        public string? opt { get; set; }
        public DateTime? optExpiry { get; set; }
        public string? verificationToken  { get; set; }
        public DateTime? verificationTokenExpiry  { get; set; }
        public DateTime? createdDate {get; set;} = DateTime.UtcNow;
        public DateTime? modifiedDate {get; set;}
    }
}