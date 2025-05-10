using Backend.Domain.Users.Entities;

namespace Backend.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Profile { get; set; }

    public static UserDto FromUser(User user)
    {
        if (user == null) 
            return null;
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Profile = user.Profile,
        };
    }
}