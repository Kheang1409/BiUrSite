using MediatR;

namespace Backend.Application.Posts.Delete;

public record DeletePostCommand(
    Guid Id,
    Guid UserId) : IRequest;
