using MediatR;

namespace Backend.Application.Features.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Unit>;