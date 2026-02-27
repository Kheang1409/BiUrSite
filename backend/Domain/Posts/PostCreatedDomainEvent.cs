using Backend.Domain.Primitive;

namespace Backend.Domain.Posts;

public record PostCreatedDomainEvent(
    Guid Id,
    PostId PostId, byte[]? Data) : DomainEvent(Id, DateTime.UtcNow);