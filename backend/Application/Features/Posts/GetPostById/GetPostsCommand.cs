using Backend.Domain.Posts.Entities;
using MediatR;

namespace Backend.Application.Features.Posts.GetPostById;

public record GetPostByIdCommand(int PostId) : IRequest<Post?>;
