using FluentValidation;

namespace Backend.Application.Features.Notifications.CreateNotification;

public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.Message).NotEmpty().WithMessage("Message is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.PostId).NotEmpty().WithMessage("PostId is required.");
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("CommentId is required.");
    }
}
