using MediatR;

namespace Backend.Application.Posts.Admin;

public record AdminDeletePostCommand(
    Guid PostId,
    string? Reason) : IRequest;
