using FluentValidation;

namespace Backend.Application.Features.Comments.CreateComment;
public class CreateCommentWithIdsCommandValidator : AbstractValidator<CreateCommentWithIdsCommand>
{
    public CreateCommentWithIdsCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .Length(1, 3000).WithMessage("Description must be between 1 and 3000 characters.");
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required.")
            .GreaterThan(0).WithMessage("User Id must be greater than 0.");
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post Id is required.")
            .GreaterThan(0).WithMessage("Post Id must be greater than 0.");
    }
}
