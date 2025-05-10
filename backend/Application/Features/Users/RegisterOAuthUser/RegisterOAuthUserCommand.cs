using MediatR;

namespace Backend.Application.Features.Users.RegisterOAuthUser;

public record RegisterOAuthUserCommand(string Email, string Username, string Password) : IRequest<Unit>;
