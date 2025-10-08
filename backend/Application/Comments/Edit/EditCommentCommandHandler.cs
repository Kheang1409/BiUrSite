using Backend.Domain.Comments;
using Backend.Domain.Posts;
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
        var postId = new PostId(request.PostId);
        var commentId = new CommentId(request.Id);
        var comment = await _commentRepository.GetCommentById(postId, commentId);
        if (comment is null)
            throw new NotFoundException("Comment is not found.");
        if(!comment.UserId.Value.Equals(request.UserId))
            throw new ForbiddenException("You are not authorized to edit this comment.");
        comment.UpdateContent(request.Text);
        await _commentRepository.Update(postId, comment);
    }
}
