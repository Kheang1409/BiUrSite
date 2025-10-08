using Backend.Domain.Comments;
using MediatR;

namespace Backend.Application.Comments.GetComment;

public record GetCommentByIdQuery(
    string PostId,
    string Id) : IRequest<Comment?>;
