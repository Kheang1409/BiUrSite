using MediatR;

namespace Backend.Application.Comments.Delete;

public record DeleteCommentCommand(
    Guid PostId,
    Guid Id,
    Guid UserId) : IRequest;
