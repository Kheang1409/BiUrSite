using Backend.Domain.Users;

namespace Backend.Application.DTOs.Users;

public class UserDto
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; }  = string.Empty;
    public string Profile { get; private set; }  = string.Empty;
    public string? Phone { get; private set; }
    public string Bio { get; private set; } = string.Empty;
    public bool HasNewNotification { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }


    public static explicit operator UserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id.Value,
            Username = user.Username,
            Email = user.Email,
            Profile = user.Profile.Url,
            Phone = user.Phone,
            Bio = user.Bio,
            HasNewNotification = user.HasNewNotification,
            CreatedDate = user.CreatedDate,
            ModifiedDate = user.ModifiedDate,
        };
    }
}