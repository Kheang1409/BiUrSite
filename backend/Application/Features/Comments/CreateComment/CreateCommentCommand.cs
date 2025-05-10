using MediatR;

namespace Backend.Application.Features.Comments.CreateComment;
public record CreateCommentCommand(string Description) : IRequest<int>;
