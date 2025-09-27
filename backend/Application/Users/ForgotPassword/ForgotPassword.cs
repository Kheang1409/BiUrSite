using MediatR;

namespace Backend.Application.Users.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest;
