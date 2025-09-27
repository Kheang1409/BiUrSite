using FluentValidation;

namespace Backend.Application.Users.Update;

public class UpdateProfileCommandValidate: AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidate()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");
    }
}
