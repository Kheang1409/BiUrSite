using FluentValidation;

namespace Backend.Application.Features.Users.DeleteUser;
public class BannerCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public BannerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User ID is required.")
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");
    }
}
