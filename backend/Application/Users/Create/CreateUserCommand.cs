using Backend.Application.Behavior;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.Create;

public record CreateUserCommand(
    string Email,
    string Username,
    string Password) : IRequest<User>, IIdempotentCommand
{
    public string IdempotencyKey => Email.ToLowerInvariant();
}