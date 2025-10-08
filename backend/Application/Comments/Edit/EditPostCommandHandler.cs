using Backend.Domain.Comments;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Comments.Edit;

public record EditCommentCommandHandler : IRequestHandler<EditCommentCommand>
{
    private readonly ICommentRepository _commentRepository;
    public EditCommentCommandHandler(
        ICommentRepository commentRepository
    )
    {
        _commentRepository = commentRepository;
    }
    public async Task Handle(EditCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetCommentById(request.PostId, request.Id);
        if (comment is null)
            throw new NotFoundException("Comment is not found.");
        if(!comment.UserId.Value.Equals(request.UserId))
            throw new ForbiddenException("You are not authorized to edit this comment.");
        comment.UpdateContent(request.Text);
        await _commentRepository.Update(request.PostId, comment);
    }
}
