using MediatR;

namespace Backend.Application.Comments.Admin;

public record AdminDeleteCommentCommand(
    Guid PostId,
    Guid CommentId,
    string? Reason) : IRequest;
