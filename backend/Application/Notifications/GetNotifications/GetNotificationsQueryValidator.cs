using FluentValidation;

namespace Backend.Application.Notifications.GetNotifications;

public class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required.");

        RuleFor(x => x.PageNumber)
            .NotEmpty().WithMessage("Page Number is required.")
            .GreaterThanOrEqualTo(1).WithMessage("Page must be atleast 1.");
    }
}
