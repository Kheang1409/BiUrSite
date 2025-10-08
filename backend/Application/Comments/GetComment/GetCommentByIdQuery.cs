using Backend.Domain.Comments;
using MediatR;

namespace Backend.Application.Comments.GetComment;

public record GetCommentByIdQuery(
    Guid PostId,
    Guid Id) : IRequest<Comment?>;
