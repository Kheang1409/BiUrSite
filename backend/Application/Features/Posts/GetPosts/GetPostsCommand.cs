using Backend.Domain.Posts.Entities;
using MediatR;

namespace Backend.Application.Features.Posts.GetPosts;

public record GetPostsCommand(string? Keyword, int PageNumber=1) : IRequest<List<Post>>;
