
namespace Backend.Application.Comments.Create;

public record CommentCreatedEvent(
    string PostId,
    string Id,
    Guid UserId
    );