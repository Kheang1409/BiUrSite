using FluentValidation;

namespace Backend.Application.Features.Posts.GetPosts;
public class GetPostsCommandValidator : AbstractValidator<GetPostsCommand>
{
    public GetPostsCommandValidator()
    {
        RuleFor(x => x.PageNumber)
            .NotEmpty().WithMessage("Page number is required.")
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Keyword)
            .MaximumLength(100).WithMessage("Keyword must not exceed 100 characters.");
    }
}