using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.ResetPassword;

public record ResetPasswordCommand(string Email, string Password, string Otp) : IRequest<User?>;
