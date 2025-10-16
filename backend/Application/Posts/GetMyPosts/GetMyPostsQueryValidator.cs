using FluentValidation;

namespace Backend.Application.Posts.GetMyPosts;

public class GetMyPostsQueryValidator : AbstractValidator<GetMyPostsQuery>
{
    public GetMyPostsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .NotEmpty().WithMessage("Page Number is required.")
            .GreaterThanOrEqualTo(1).WithMessage("Page must be atleast 1.");
    }
}
