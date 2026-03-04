using FluentValidation;

namespace Backend.Application.Users.Admin;

public class BanUserCommandValidator : AbstractValidator<BanUserCommand>
{
    public BanUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be greater than 0 minutes.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Reason))
            .WithMessage("Ban reason must not exceed 500 characters.");
    }
}
