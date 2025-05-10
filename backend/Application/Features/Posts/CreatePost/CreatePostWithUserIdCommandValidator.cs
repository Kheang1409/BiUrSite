using FluentValidation;

namespace Backend.Application.Features.Posts.CreatePost;
public class CreatePostWithUserIdCommandValidator : AbstractValidator<CreatePostWithUserIdCommand>
{
    public CreatePostWithUserIdCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .Length(1, 3000).WithMessage("Description must be between 1 and 3000 characters.");
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");
    }
}