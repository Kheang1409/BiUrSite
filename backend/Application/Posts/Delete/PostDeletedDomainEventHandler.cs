using Backend.Domain.Posts;
using MediatR;
using Rebus.Bus;

namespace Backend.Application.Posts.Delete;

internal sealed class PostDeletedDomainEventHandler : INotificationHandler<PostDeletedDomainEvent>
{
    private readonly IBus _bus;

    public PostDeletedDomainEventHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(PostDeletedDomainEvent request, CancellationToken cancellationToken)
    {
        await _bus.Send(new PostDeletedEvent(request.PostId, request.Image));
    }
}
