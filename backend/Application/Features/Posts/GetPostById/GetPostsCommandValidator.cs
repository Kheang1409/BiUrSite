using FluentValidation;

namespace Backend.Application.Features.Posts.GetPostById;
public class GetPostByIdCommandValidator : AbstractValidator<GetPostByIdCommand>
{
    public GetPostByIdCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.")
            .GreaterThan(0).WithMessage("Post ID must be greater than 0.");
    }
}