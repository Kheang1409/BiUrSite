using MediatR;

namespace Backend.Application.Features.Posts.UpdatePost;

public record UpdatePostWithPostIdCommand(int PostId, string Description) : IRequest<bool>;
