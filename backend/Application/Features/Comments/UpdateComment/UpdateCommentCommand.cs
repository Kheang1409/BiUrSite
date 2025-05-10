using MediatR;

namespace Backend.Application.Features.Comments.UpdateComment;
public record UpdateCommentCommand(string Description) : IRequest<bool>;
