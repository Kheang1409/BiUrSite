using Backend.Domain.Posts;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Posts.GetPosts;

public record GetPostsQuery(
    string? Keywords,
    int PageNumber=1) : IRequest<IEnumerable<Post>>;
