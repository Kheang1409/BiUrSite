using MediatR;

namespace Backend.Application.Posts.Edit;

public record EditPostCommand(
    Guid Id,
    Guid UserId,
    string Text,
    byte[]? Data = null,
    bool RemoveImage = false) : IRequest;
