using FluentValidation;

namespace Backend.Application.Features.Comments.DeleteComment;
public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("Comment Id is required.")
            .GreaterThan(0).WithMessage("Comment Id must be greater than 0.");
    }
}
