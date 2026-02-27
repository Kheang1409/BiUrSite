using Backend.Domain.Users;
using MediatR;
using Rebus.Bus;

namespace Backend.Application.Users.Update;

internal sealed class UpdatedProfileDomainEventHandler : INotificationHandler<UpdatedProfileDomainEvent>
{
    private readonly IBus _bus;

    public UpdatedProfileDomainEventHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(UpdatedProfileDomainEvent request, CancellationToken cancellationToken)
    {
        await _bus.Send(new UpdatedProfileEvent(request.UserId.Value, request.Data));
    }
}
