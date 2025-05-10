using FluentValidation;

namespace Backend.Application.Features.Notifications.GetNofications;
public class GetNoficationsCommandValidator : AbstractValidator<GetNoficationsCommand>
{
    public GetNoficationsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required.")
            .GreaterThan(0).WithMessage("User Id must be greater than 0.");
    }
}