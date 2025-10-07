using Backend.Domain.Primitive;

namespace Backend.Domain.Posts;

public record PostDeletedDomainEvent(
    Guid Id,
    string PostId,
    Image Image) : DomainEvent(Id, DateTime.UtcNow);