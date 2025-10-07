using Backend.Domain.Posts;
using MediatR;

namespace Backend.Application.Posts.Create;

public record CreatePostCommand(
    Guid UserId,
    string Username,
    string Text,
    byte[]? Data) : IRequest<Post>;