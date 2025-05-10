using FluentValidation;

namespace Backend.Application.Features.Users.RegisterOAuthUser;

public class RegisterOAuthUserCommandValidator : AbstractValidator<RegisterOAuthUserCommand>
{
    public RegisterOAuthUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(2, 50).WithMessage("Name must be at least 2 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be valid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.");
    }
}
