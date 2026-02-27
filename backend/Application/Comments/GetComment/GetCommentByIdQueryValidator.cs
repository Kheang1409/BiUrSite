using FluentValidation;

namespace Backend.Application.Comments.GetComment;

public class GetCommentByIdQueryValidator : AbstractValidator<GetCommentByIdQuery>
{
    public GetCommentByIdQueryValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post Id is required.");
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Comment Id is required.");
    }
}
