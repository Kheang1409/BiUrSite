using Backend.Application.Behavior;
using Backend.Domain.Posts;
using MediatR;

namespace Backend.Application.Posts.Create;

public record CreatePostCommand(
    Guid UserId,
    string Username,
    string Text,
    byte[]? Data,
    string? ClientIdempotencyKey = null) : IRequest<Post>, IIdempotentCommand
{
    public string IdempotencyKey => ClientIdempotencyKey ?? $"{UserId}:{Text.GetHashCode()}";
}