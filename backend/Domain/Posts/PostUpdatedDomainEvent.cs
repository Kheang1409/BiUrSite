using Backend.Domain.Users;
using Backend.Domain.Primitive;

namespace Backend.Domain.Posts;

public record PostUpdatedDomainEvent(
    Guid Id,
    PostId PostId,
    UserId UserId,
    string? Text,
    string? ImageUrl) : DomainEvent(Id, DateTime.UtcNow);
