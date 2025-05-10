using MediatR;

namespace Backend.Application.Features.Users.RegisterUser;

public record RegisterUserCommand(string Email, string Username, string Password) : IRequest<Unit>;