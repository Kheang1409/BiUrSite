using Backend.Domain.Comments.Entities;
using Backend.Domain.Comments.Interfaces;
using MediatR;

namespace Backend.Application.Features.Comments.GetCommentById;
public class GetCommentsWithPostIdCommandHandler : IRequestHandler<GetCommentByIdCommand, Comment?>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentsWithPostIdCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Comment?> Handle(GetCommentByIdCommand request, CancellationToken cancellationToken)
    {
        var comments = await _commentRepository.GetCommentByIdAsync(request.CommentId);
        return comments;
    }
}
