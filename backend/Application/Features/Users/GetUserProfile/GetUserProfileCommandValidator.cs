using FluentValidation;

namespace Backend.Application.Features.Users.GetUserProfile;
public class GetUserProfileCommandValidator : AbstractValidator<GetUserProfileCommand>
{
    public GetUserProfileCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
