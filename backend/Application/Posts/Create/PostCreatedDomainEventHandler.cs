using Backend.Domain.Posts;
using MediatR;
using Rebus.Bus;

namespace Backend.Application.Posts.Create;

internal sealed class PostCreatedDomainEventHandler : INotificationHandler<PostCreatedDomainEvent>
{
    private readonly IBus _bus;

    public PostCreatedDomainEventHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(PostCreatedDomainEvent request, CancellationToken cancellationToken)
    {
        await _bus.Send(new PostCreatedEvent(request.PostId.Value, request.Data));
    }
}
