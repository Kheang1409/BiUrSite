using MediatR;

namespace Backend.Domain.Primitive;

/// <summary>
/// Base record for domain events. Includes an OccurredOn timestamp to help ordering.
/// Implements MediatR's INotification so the infrastructure can publish events.
/// </summary>
public record DomainEvent(Guid Id, DateTime OccurredOn) : INotification;