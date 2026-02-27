using Backend.Domain.Comments;
using MediatR;

namespace Backend.Application.Comments.GetComments;

public record GetCommentsQuery(
    Guid PostId,
    int PageNumber=1) : IRequest<IEnumerable<Comment>>;
