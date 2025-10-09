namespace Backend.Application.DTOs.Users;

public record UpdateProfileDto(
    string Username,
    string Bio,
    byte[]? Data
);