using FluentValidation;

namespace Backend.Application.Comments.Edit;

public class EditCommentCommandValidator: AbstractValidator<EditCommentCommand>
{
    public EditCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post Id is required.");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Comment Id is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MinimumLength(1).WithMessage("Text must be at least 1 characters. ");
    }
}
