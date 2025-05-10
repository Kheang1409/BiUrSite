using FluentValidation;

namespace Backend.Application.Features.Posts.DeletePost;
public class DeletePostCommandValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.")
            .GreaterThan(0).WithMessage("Post ID must be greater than 0.");
    }
}