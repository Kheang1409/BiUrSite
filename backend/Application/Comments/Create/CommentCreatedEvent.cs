
namespace Backend.Application.Comments.Create;

public record CommentCreatedEvent(
    Guid PostId,
    Guid Id,
    Guid UserId
    );