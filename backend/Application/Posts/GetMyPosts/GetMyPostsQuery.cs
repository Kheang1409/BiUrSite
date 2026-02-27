using Backend.Domain.Posts;
using MediatR;

namespace Backend.Application.Posts.GetMyPosts;

public record GetMyPostsQuery(
    Guid UserId,
    int PageNumber = 1) : IRequest<IEnumerable<Post>>;
