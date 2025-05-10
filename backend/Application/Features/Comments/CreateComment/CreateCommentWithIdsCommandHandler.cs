using Backend.Domain.Comments.Entities;
using Backend.Domain.Comments.Interfaces;
using MediatR;

namespace Backend.Application.Features.Comments.CreateComment;
public class CreateCommentWithIdsCommandHandler : IRequestHandler<CreateCommentWithIdsCommand, Comment>
{
    private readonly ICommentRepository _commentRepository;
    public CreateCommentWithIdsCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Comment> Handle(CreateCommentWithIdsCommand request, CancellationToken cancellationToken)
    {
        var comment = Comment.Create(request.UserId, request.PostId, request.Description);
        comment = await _commentRepository.CreateCommentAsync(comment);
        if(comment == null)
            throw new Exception("unable to comment!");
        return comment;
    }
}
