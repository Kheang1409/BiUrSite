using FluentValidation;

namespace Backend.Application.Features.Comments.GetCommentById;
public class GetCommentsWithPostIdCommandValidator : AbstractValidator<GetCommentByIdCommand>
{
    public GetCommentsWithPostIdCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("Comment Id is required.")
            .GreaterThan(0).WithMessage("Comment Id must be greater than 0.");
    }
}
