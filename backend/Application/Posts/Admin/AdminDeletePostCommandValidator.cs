using FluentValidation;

namespace Backend.Application.Posts.Admin;

public class AdminDeletePostCommandValidator : AbstractValidator<AdminDeletePostCommand>
{
    public AdminDeletePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Reason))
            .WithMessage("Reason must not exceed 500 characters.");
    }
}
