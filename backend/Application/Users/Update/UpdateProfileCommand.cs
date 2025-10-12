using MediatR;

namespace Backend.Application.Users.Update;

public record UpdateProfileCommand(
    string Email,
    string Username,
    string? Bio,
    byte[]? Data) : IRequest;
