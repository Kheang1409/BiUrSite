using MediatR;

namespace Backend.Application.Features.Posts.CountUserTotalPost;

public record CountUserTotalPostCommand(int UserId) : IRequest<int>;
