using Backend.Domain.Comments.Interfaces;
using MediatR;

namespace Backend.Application.Features.Comments.DeleteComment;
public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly ICommentRepository _commentRepository;

    public DeleteCommentCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        return await _commentRepository.SoftDeleteCommentAsync(request.CommentId);
    }
}
