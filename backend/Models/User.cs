using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Backend.Enums;

namespace Backend.Models{
    public class User{
        [Key]
        public int userId {get; set;}
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(maximumLength: 15, MinimumLength = 2, ErrorMessage = "Username must be between 2 and 15 characters.")]
        public required string username {get; set;}
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string email {get; set;}
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string password { get; set; }
        public string? profile {get; set;} ="assets/img/profile-default.svg";
        [Required]
        public bool isActive {get; set;} = false;
        [Required]
        public Status status {get; set;} = Status.Unverified;
        [Required]
        public Role role {get; set; } = Role.User;
        public string? otp { get; set; }
        public DateTime? otpExpiry { get; set; }
        public string? verificationToken  { get; set; }
        public DateTime? verificationTokenExpiry  { get; set; }
        public DateTime? createdDate {get; set;}
        public DateTime? modifiedDate {get; set;}
        public DateTime? deletedDate { get; set; }
        [JsonIgnore]
        public List<Post>? posts {get; set;} = new List<Post>();
        public List<Comment>? comments {get; set;} = new List<Comment>();

        public void GenerateOtp()
        {
            otp = new Random().Next(100000, 999999).ToString();
            otpExpiry = DateTime.UtcNow.AddMinutes(3);
        }

        public User GenerateVerfiedToken()
        {
            verificationToken = Guid.NewGuid().ToString();
            verificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            return this;
        }

        public static string HashPassword(string plainPassword)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            return hashedPassword;
        }

        public bool VerifyPassword(string plainPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, password);
        }
    }
}