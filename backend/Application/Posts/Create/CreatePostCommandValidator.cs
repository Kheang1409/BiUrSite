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

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Text) || (x.Data?.Length ?? 0) > 0)
            .WithMessage("Post must contain text or image.");
    }
}
