using FluentValidation;

namespace Backend.Application.Features.Comments.GetComments;
public class GetCommentsWithPostIdCommandValidator : AbstractValidator<GetCommentsWithPostIdCommand>
{
    public GetCommentsWithPostIdCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.")
            .GreaterThan(0).WithMessage("Post ID must be greater than 0.");

        RuleFor(x => x.PageNumber)
            .NotEmpty().WithMessage("Page Number is required.")
            .GreaterThan(0).WithMessage("Page Number must be greater than 0.");
    }
}
