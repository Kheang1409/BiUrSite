using FluentValidation;

namespace Backend.Application.Features.Posts.CountUserTotalPost;
public class CountUserTotalPostCommandValidator : AbstractValidator<CountUserTotalPostCommand>
{
    public CountUserTotalPostCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId is required.");
    }
}