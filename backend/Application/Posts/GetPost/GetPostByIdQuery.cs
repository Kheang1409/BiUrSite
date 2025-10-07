using Backend.Domain.Posts;
using MediatR;

namespace Backend.Application.Posts.GetPost;

public record GetPostByIdQuery(string Id) : IRequest<Post?>;
