using FluentValidation;

namespace Backend.Application.Posts.GetPosts;

public class GetPostsQueryValidator : AbstractValidator<GetPostsQuery>
{
    public GetPostsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .NotEmpty().WithMessage("Page Number is required.")
            .GreaterThanOrEqualTo(1).WithMessage("Page must be atleast 1.");
    }
}
