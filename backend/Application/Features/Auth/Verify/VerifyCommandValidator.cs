using Backend.Application.Features.Auth.Verify;
using FluentValidation;

namespace Backend.Application.Features.Auth.Login;

public class VerifyCommandValidator : AbstractValidator<VerifyCommand>
{
    public VerifyCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be valid.");
    }
}
