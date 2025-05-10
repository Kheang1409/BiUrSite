using Backend.Domain.Comments.Interfaces;
using MediatR;

namespace Backend.Application.Features.Comments.UpdateComment;
public class UpdateCommentWithIdCommandHandler : IRequestHandler<UpdateCommentWithIdCommand, bool>
{
    private readonly ICommentRepository _commentRepository;
    public UpdateCommentWithIdCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<bool> Handle(UpdateCommentWithIdCommand request, CancellationToken cancellationToken)
    {
        return await _commentRepository.UpdateCommentAsync(request.CommentId, request.Description);;
    }
}
