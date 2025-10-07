
using Backend.Domain.Primitive;

namespace Backend.Application.Data;
public interface IDomainEventDispatcher
{
    Task DispatchAsync(Entity entity, CancellationToken cancellationToken = default);
}
