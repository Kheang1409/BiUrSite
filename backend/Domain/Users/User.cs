
using System.Security.Cryptography;
using System.Text;
using Backend.Domain.Enums;
using Backend.Domain.Images;
using Backend.Domain.Primitive;

namespace Backend.Domain.Users;

public class User : Entity
{
    private const string DEFAULT_PROFILE = "https://raw.githubusercontent.com/Kheang1409/images/refs/heads/main/profiles/profile-default.webp";
    public UserId Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Image? Profile { get; private set; }
    public string Bio { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public string AuthProvider { get; private set; } = string.Empty;
    public Status Status { get; private set; }
    public Role Role { get; private set; }
    public Otp? Otp { get; private set; }
    public Token? Token { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    private User(){}
    private User(Builder builder)
    {
        Id = builder.Id ?? new UserId(Guid.NewGuid());
        Username = builder.Username;
        Email = builder.Email;
        Password = HashPassword(builder.Password);
        Status = builder.Status;
        Role = builder.Role;
        AuthProvider = builder.AuthProvider;
        Bio = builder.Bio;
        Token = builder.Token;
        Profile = new Image(DEFAULT_PROFILE);
        CreatedDate = DateTime.UtcNow;
    }

    public class Builder
    {
        internal UserId? Id;
        internal string Username  = string.Empty;
        internal string Email  = string.Empty; 
        internal string Bio  = string.Empty;
        internal string Password  = string.Empty;
        internal string AuthProvider  = string.Empty;
        internal Status Status; 
        internal Role Role;
        internal Token? Token; 

        public Builder SetId(UserId? id)
        {
            Id = id;
            return this;
            
        }

        public Builder SetUserName(string username)
        {
            Username = username;
            return this;
        }
        public Builder SetEmail(string email)
        {
            Email = email;
            return this;
        }
        public Builder SetPassword(string password)
        {
            Password = password;
            return this;
        }
        public Builder SetBio(string bio)
        {
            Bio = bio;
            return this;
        }

        public Builder SetAuthProvider(string authProvider)
        {
            AuthProvider = authProvider;
            return this;
        }
        
        public Builder SetStatus(Status status)
        {
            Status = status;
            return this;
        }
        public Builder SetRole(Role role)
        {
            Role = role;
            return this;
        }

        public Builder SetToken(Token? token)
        {
            Token = token;
            return this;
        }

        public User Build()
        {
            if(string.IsNullOrWhiteSpace(Username))
                throw new InvalidOperationException("Username cannot be empty.");
            if(string.IsNullOrWhiteSpace(Email))
                throw new InvalidOperationException("Email cannot be empty.");
            if(string.IsNullOrWhiteSpace(Password) && string.IsNullOrWhiteSpace(AuthProvider))
                throw new InvalidOperationException("Password cannot be empty.");
            var user = new User(this);
            user.AddDomainEvent(new UserCreatedDomainEvent(Guid.NewGuid(), user.Id));
            return user;
        }
    }

    private static string HashPassword(string plainPassword)
    {
        using (SHA512 sha512Hash = SHA512.Create())
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            byte[] passwordBytes = Encoding.UTF8.GetBytes(plainPassword);

            byte[] saltedPassword = passwordBytes.Concat(salt).ToArray();

            byte[] hashBytes = sha512Hash.ComputeHash(saltedPassword);

            byte[] hashWithSalt = salt.Concat(hashBytes).ToArray();

            return Convert.ToBase64String(hashWithSalt);
        }
    }

        public void IsExistingNotActivated(string username, string plainPassword)
        {
            Username = username;
            Password = HashPassword(plainPassword);
            Token = Token.Generate();
            AddDomainEvent(new UserCreatedDomainEvent(Guid.NewGuid(), Id));
        }

    public User ResetUsername(string username)
    {
        Username = username;
        return this;
    }

    public void ReCreate()
    {
        AddDomainEvent(new UserCreatedDomainEvent(Guid.NewGuid(), Id));
    }

    public void Update(string username, string bio, byte[] data)
    {
        Username = username;
        Bio = bio;
        AddDomainEvent(new UpdatedProfileDomainEvent(Guid.NewGuid(), Id, data));
    }

    public void Verify()
    {
        Token = null;
        Status = Status.Active;
    }

    public User ResetPassword(string plainPassword)
    {
        Password = HashPassword(plainPassword);
        Otp = null;
        return this;
    }

    public void ForgotPassword()
    {
        Otp = Otp.Generate();
        AddDomainEvent(new UserForgotPasswordDomainEvent(Guid.NewGuid(), Id));
    }

    public void Delete()
    {
        Status = Status.Deleted;
        DeletedDate = DateTime.UtcNow;
    }

    public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        byte[] hashWithSalt = Convert.FromBase64String(hashedPassword);
        byte[] salt = new byte[16];
        Array.Copy(hashWithSalt, 0, salt, 0, 16);

        byte[] hashBytes = new byte[hashWithSalt.Length - 16];
        Array.Copy(hashWithSalt, 16, hashBytes, 0, hashBytes.Length);

        using (SHA512 sha512Hash = SHA512.Create())
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(plainPassword);
            byte[] saltedPassword = passwordBytes.Concat(salt).ToArray();
            byte[] computedHash = sha512Hash.ComputeHash(saltedPassword);

            return hashBytes.SequenceEqual(computedHash);
        }
    }

    public void SetImage(string url)
    {
        Profile = new Image(url);
        ModifiedDate = DateTime.UtcNow;
    }
}
