
using MediatR;

namespace Backend.Application.Users.CreateByOAuth;

public record CreateUserByOAuthCommand(
    Guid Id,
    string Email,
    string Username,
    string AuthProvider) : IRequest;
