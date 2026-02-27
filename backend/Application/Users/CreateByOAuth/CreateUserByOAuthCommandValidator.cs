using FluentValidation;

namespace Backend.Application.Users.CreateByOAuth;

public class CreateUserByOAuthCommandValidator : AbstractValidator<CreateUserByOAuthCommand>
{
    public CreateUserByOAuthCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(2, 50).WithMessage("Name must be at least 2 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be valid.");
    }
}
