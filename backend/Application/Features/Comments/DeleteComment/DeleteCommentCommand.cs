using MediatR;

namespace Backend.Application.Features.Comments.DeleteComment;
public record DeleteCommentCommand(int CommentId) : IRequest<bool>;