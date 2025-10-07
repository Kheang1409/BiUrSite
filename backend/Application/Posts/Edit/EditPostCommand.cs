using MediatR;

namespace Backend.Application.Posts.Edit;

public record EditPostCommand(
    string Id,
    Guid UserId,
    string Text) : IRequest;
