using Domain.Primitive;

namespace Backend.Domain.Users;

public record UserCreatedDomainEvent(
    Guid Id,
    UserId UserId): DomainEvent(Id);