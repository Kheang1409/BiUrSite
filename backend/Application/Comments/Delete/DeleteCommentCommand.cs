using MediatR;

namespace Backend.Application.Comments.Delete;

public record DeleteCommentCommand(
    string PostId,
    string Id,
    Guid UserId) : IRequest;
