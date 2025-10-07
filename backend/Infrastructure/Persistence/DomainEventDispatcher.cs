using Backend.Application.Data;
using Backend.Domain.Primitive;
using MediatR;

namespace Backend.Infrastructure.Persistence;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public DomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(Entity entity, CancellationToken cancellationToken = default)
    {
        var domainEvents = entity.GetDomainEvents().ToList();
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        entity.ClearDomainEvents();
    }
}
