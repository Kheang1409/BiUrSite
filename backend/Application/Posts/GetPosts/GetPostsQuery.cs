using Backend.Domain.Posts;
using MediatR;

namespace Backend.Application.Posts.GetPosts;

public record GetPostsQuery(
    string? Username,
    string? Keywords,
    int PageNumber=1) : IRequest<IEnumerable<Post>>;
