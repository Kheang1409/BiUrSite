using FluentValidation;

namespace Backend.Application.Users.ForgotPassword;

public class LoginCommandHandlerValidator : AbstractValidator<ForgotPasswordCommand>
{
    public LoginCommandHandlerValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be valid.");
    }
}