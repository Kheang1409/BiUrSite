using MediatR;

namespace Backend.Application.Comments.Edit;

public record EditCommentCommand(
    Guid PostId,
    Guid Id,
    Guid UserId,
    string Text) : IRequest;
