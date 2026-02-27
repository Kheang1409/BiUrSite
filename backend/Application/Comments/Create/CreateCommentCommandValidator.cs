using FluentValidation;

namespace Backend.Application.Comments.Create;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post Id is required.");
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required.");
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MinimumLength(1).WithMessage("Text is required.");
    }
}
