using MediatR;

namespace Backend.Domain.Primitive;
public record DomainEvent(Guid Id, DateTime OccurredOn) : INotification;