using FluentValidation;

namespace Backend.Application.Users.Update;

public class UpdateProfileCommandValidator: AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");
    }
}
