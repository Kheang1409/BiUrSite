using MediatR;

namespace Backend.Application.Features.Posts.DeletePost;

public record DeletePostCommand(int PostId) : IRequest<bool>;
