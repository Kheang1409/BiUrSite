using FluentValidation;

namespace Backend.Application.Posts.Create;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(2, 25).WithMessage("Username must be at least 2 characters.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MinimumLength(1).WithMessage("Text must be at least 1 characters.");
    }
}
