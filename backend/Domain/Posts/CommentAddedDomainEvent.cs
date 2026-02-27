using Backend.Domain.Comments;
using Backend.Domain.Primitive;
using Backend.Domain.Users;

namespace Backend.Domain.Posts;

public record CommentAddedDomainEvent(
    Guid Id,
    PostId PostId,
    CommentId CommentId,
    UserId UserId) : DomainEvent(Id, DateTime.UtcNow);