using MediatR;

namespace Backend.Application.Comments.Edit;

public record EditCommentCommand(
    string PostId,
    string Id,
    Guid UserId,
    string Text) : IRequest;
