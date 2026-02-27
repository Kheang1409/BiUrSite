using Backend.Domain.Primitive;

namespace Backend.Application.Data;

public interface IUnitOfWork
{
    Task SaveChangesAsync(Entity entity, CancellationToken cancellationToken);
}