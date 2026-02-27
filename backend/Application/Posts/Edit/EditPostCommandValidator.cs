using FluentValidation;

namespace Backend.Application.Posts.Edit;

public class EditPostCommandValidator: AbstractValidator<EditPostCommand>
{
    public EditPostCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Post Id is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MinimumLength(1).WithMessage("Text must be at least 1 characters. ");
    }
}
