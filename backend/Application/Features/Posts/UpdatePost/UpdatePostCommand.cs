using MediatR;

namespace Backend.Application.Features.Posts.UpdatePost;

public record UpdatePostCommand(string Description) : IRequest<bool>;
