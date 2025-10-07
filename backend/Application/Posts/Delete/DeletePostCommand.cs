using MediatR;

namespace Backend.Application.Posts.Delete;

public record DeletePostCommand(
    string Id,
    Guid UserId) : IRequest;
