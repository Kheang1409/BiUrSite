using Backend.Domain.Primitive;

namespace Backend.Domain.Users;

public record UpdatedProfileDomainEvent(
    Guid Id,
    UserId UserId,
    byte[] Data) : DomainEvent(Id, DateTime.UtcNow);