using Backend.Domain.Primitive;
using Backend.Domain.Users;

namespace Backend.Domain.Posts;

public record CommentAddedDomainEvent(
    Guid Id,
    string PostId,
    string CommentId,
    UserId UserId) : DomainEvent(Id, DateTime.UtcNow);