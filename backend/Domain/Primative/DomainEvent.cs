using MediatR;

namespace Domain.Primitive;

public record DomainEvent(Guid Id) : INotification;