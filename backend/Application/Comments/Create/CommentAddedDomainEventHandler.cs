using Backend.Domain.Posts;
using MediatR;
using Rebus.Bus;

namespace Backend.Application.Comments.Create;

internal sealed class CommentAddedDomainEventHandler : INotificationHandler<CommentAddedDomainEvent>
{
    private readonly IBus _bus;

    public CommentAddedDomainEventHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(CommentAddedDomainEvent request, CancellationToken cancellationToken)
    {
        await _bus.Send(new CommentCreatedEvent(request.PostId.Value, request.CommentId.Value, request.UserId.Value));
    }
}
