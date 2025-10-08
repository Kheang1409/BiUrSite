using Backend.Domain.Primitive;

namespace Backend.Domain.Posts;

public record PostDeletedDomainEvent(
    Guid Id,
    PostId PostId,
    Image Image) : DomainEvent(Id, DateTime.UtcNow);