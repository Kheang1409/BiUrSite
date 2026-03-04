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
    public string Role { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string? BanReason { get; private set; }
    public DateTime? BanEndDate { get; private set; }
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
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            BanReason = user.BanReason,
            BanEndDate = user.BanEndDate,
            CreatedDate = user.CreatedDate,
            ModifiedDate = user.ModifiedDate,
        };
    }

    public static UserDto Create(User user, Guid? requesterId, bool requesterIsAdmin)
    {
        var includePrivate = requesterIsAdmin || (requesterId.HasValue && user.Id.Value.Equals(requesterId.Value));
        return new UserDto
        {
            Id = user.Id.Value,
            Username = user.Username,
            Email = includePrivate ? user.Email : string.Empty,
            Profile = user.Profile.Url,
            Phone = includePrivate ? user.Phone : null,
            Bio = user.Bio,
            HasNewNotification = user.HasNewNotification,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            BanReason = requesterIsAdmin ? user.BanReason : null,
            BanEndDate = requesterIsAdmin ? user.BanEndDate : null,
            CreatedDate = user.CreatedDate,
            ModifiedDate = user.ModifiedDate,
        };
    }
}