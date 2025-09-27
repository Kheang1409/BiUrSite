using Backend.Domain.Users;
using MediatR;
using Rebus.Bus;

namespace Backend.Application.Users.CreateUser;

internal sealed class UserCreatedDomainEventHandler : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly IBus _bus;

    public UserCreatedDomainEventHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(UserCreatedDomainEvent request, CancellationToken cancellationToken)
    {
        await _bus.Send(new UserCreatedEvent(request.UserId.Value));
    }
}
