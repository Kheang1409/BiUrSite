using FluentValidation;

namespace Backend.Application.Comments.Admin;

public class AdminDeleteCommentCommandValidator : AbstractValidator<AdminDeleteCommentCommand>
{
    public AdminDeleteCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.");

        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("Comment ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Reason))
            .WithMessage("Reason must not exceed 500 characters.");
    }
}
