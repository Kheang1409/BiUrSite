using Backend.Domain.Comments;
using MediatR;

namespace Backend.Application.Comments.Create;

public record CreateCommentCommand(
    string PostId,
    Guid UserId,
    string Username,
    string Text) : IRequest<Comment>;