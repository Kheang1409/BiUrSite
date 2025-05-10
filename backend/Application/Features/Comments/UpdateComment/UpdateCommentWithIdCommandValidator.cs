using FluentValidation;

namespace Backend.Application.Features.Comments.UpdateComment;
public class UpdateCommentWithIdCommandValidator : AbstractValidator<UpdateCommentWithIdCommand>
{
    public UpdateCommentWithIdCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .Length(1, 3000).WithMessage("Description must be between 1 and 3000 characters.");
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("Comment Id is required.")
            .GreaterThan(0).WithMessage("Comment Id must be greater than 0.");
    }
}
