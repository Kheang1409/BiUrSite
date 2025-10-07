using Backend.Domain.Primitive;

namespace Backend.Domain.Posts;

public record PostCreatedDomainEvent(
    Guid Id,
    string PostId, byte[]? Data) : DomainEvent(Id, DateTime.UtcNow);