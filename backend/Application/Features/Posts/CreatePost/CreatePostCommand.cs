using Backend.Domain.Posts.Entities;
using MediatR;

namespace Backend.Application.Features.Posts.CreatePost;

public record CreatePostCommand(string Description) : IRequest<Post>;
