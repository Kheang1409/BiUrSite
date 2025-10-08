using Backend.Domain.Comments;
using MediatR;

namespace Backend.Application.Comments.Create;

public record CreateCommentCommand(
    Guid PostId,
    Guid UserId,
    string Username,
    string Text) : IRequest<Comment>;