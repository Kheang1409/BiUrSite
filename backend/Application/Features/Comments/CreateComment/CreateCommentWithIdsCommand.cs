using Backend.Domain.Comments.Entities;
using MediatR;

namespace Backend.Application.Features.Comments.CreateComment;
public record CreateCommentWithIdsCommand(int PostId, int UserId, string Description) : IRequest<Comment>;
