using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.Login;

public record LoginCommand(string Email, string Password) : IRequest<User?>;
