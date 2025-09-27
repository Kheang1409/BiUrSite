using Backend.Domain.Users;
using MediatR;
using Rebus.Bus;

namespace Backend.Application.Users.ForgotPassword;

internal sealed class UserForgotPasswordDomainEventHandler : INotificationHandler<UserForgotPasswordDomainEvent>
{
    private readonly IBus _bus;

    public UserForgotPasswordDomainEventHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(UserForgotPasswordDomainEvent request, CancellationToken cancellationToken)
    {
        await _bus.Send(new UserForgotPasswordEvent(request.UserId.Value));
    }
}
