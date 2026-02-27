using FluentValidation;

namespace Backend.Application.Users.GetUsers;

public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
       RuleFor(x => x.PageNumber)
            .NotEmpty().WithMessage("Page Number is required.")
            .GreaterThanOrEqualTo(1).WithMessage("Page must be atleast 1.");
    }
}
