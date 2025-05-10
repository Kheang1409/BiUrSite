using MediatR;

namespace Backend.Application.Features.Auth.ResetPassword;

public record ResetPasswordCommand(string Opt, string NewPassword ) : IRequest<bool>;