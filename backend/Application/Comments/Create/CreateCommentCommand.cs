using Backend.Application.Behavior;
using Backend.Domain.Comments;
using MediatR;

namespace Backend.Application.Comments.Create;

public record CreateCommentCommand(
    Guid PostId,
    Guid UserId,
    string Text,
    string? ClientIdempotencyKey = null) : IRequest<Comment>, IIdempotentCommand
{
    public string IdempotencyKey => ClientIdempotencyKey ?? $"{PostId}:{UserId}:{Text.GetHashCode()}";
}