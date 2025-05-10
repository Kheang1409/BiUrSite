using Backend.Domain.Posts.Entities;
using MediatR;

namespace Backend.Application.Features.Posts.CreatePost;

public record CreatePostWithUserIdCommand(string Description, int UserId) : IRequest<Post>;
