using MediatR;

namespace Backend.Application.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<string>;