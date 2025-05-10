using FluentValidation;

namespace Backend.Application.Features.Posts.UpdatePost;
public class UpdatePostWithPostIdCommandValidator : AbstractValidator<UpdatePostWithPostIdCommand>
{
    public UpdatePostWithPostIdCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .Length(1, 3000).WithMessage("Description must be between 1 and 3000 characters.");
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.")
            .GreaterThan(0).WithMessage("Post ID must be greater than 0.");
    }
}