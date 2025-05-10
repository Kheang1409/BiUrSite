namespace Backend.Domain.Users.Entities;

using Backend.Domain.Comments.Entities;
using Backend.Domain.Common.Enums;
using Backend.Domain.Posts.Entities;
using Backend.Domain.Users.Enums;
using System.Text.Json.Serialization;

public class User
{
    public int Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    public string Profile { get; private set; }
    public Status Status { get; private set; }
    public Role Role { get; private set; }
    public string? Otp { get; private set; }
    public DateTime? OtpExpiry { get; private set; }
    public string? VerificationToken { get; private set; }
    public DateTime? VerificationTokenExpiry { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    
    [JsonIgnore]
    public List<Post>? Posts { get; set; } = new();
    public List<Comment>? Comments { get; set; } = new();

    public User(string username, string email, string password)
    {
        Username = username;
        Email = email;
        Profile = "assets/img/profile-default.svg";
        Password = HashPassword(password);
        CreatedDate = DateTime.UtcNow;
        Status = Status.Unverified;
        Role = Role.User;
    }

    public void GenerateOtp()
    {
        Otp = new Random().Next(100000, 999999).ToString();
        OtpExpiry = DateTime.UtcNow.AddMinutes(3);
    }

    public void GenerateVerificationToken()
    {
        VerificationToken = Guid.NewGuid().ToString();
        VerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
    }

    public static string HashPassword(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword);

    public bool VerifyPassword(string plainPassword) =>
        BCrypt.Net.BCrypt.Verify(plainPassword, Password);

    public void Activate()
    {
        Status = Status.Active;
    }
}
