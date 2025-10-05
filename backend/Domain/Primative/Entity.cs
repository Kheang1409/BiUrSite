
namespace Backend.Domain.Primitive;

/// <summary>
/// Base entity for domain objects.
/// Provides domain-event handling utilities used by the application to publish events after persistence.
/// Keep identity on concrete entities (e.g. UserId) so this base remains lightweight.
/// </summary>
public abstract class Entity
{
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Read-only collection of raised domain events.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Add a domain event to be published later by the infrastructure (e.g. in a UnitOfWork/DbContext after commit).
    /// </summary>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remove a previously added domain event.
    /// </summary>
    protected void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clear all domain events (used after publishing).
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}