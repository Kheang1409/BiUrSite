using MediatR;

namespace Backend.Application.Features.Comments.UpdateComment;
public record UpdateCommentWithIdCommand(int CommentId, string Description) : IRequest<bool>;
