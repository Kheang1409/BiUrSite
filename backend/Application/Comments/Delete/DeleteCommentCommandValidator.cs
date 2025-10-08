using FluentValidation;

namespace Backend.Application.Comments.Delete;

public class DeleteCommentCommandValidator: AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post Id is required.");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Comment Id is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required.");
    }
}
