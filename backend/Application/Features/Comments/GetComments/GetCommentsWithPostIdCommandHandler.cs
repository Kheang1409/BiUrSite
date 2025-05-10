using Backend.Domain.Comments.Entities;
using Backend.Domain.Comments.Interfaces;
using MediatR;

namespace Backend.Application.Features.Comments.GetComments;
public class GetCommentsWithPostIdCommandHandler : IRequestHandler<GetCommentsWithPostIdCommand, List<Comment>>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentsWithPostIdCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<List<Comment>> Handle(GetCommentsWithPostIdCommand request, CancellationToken cancellationToken)
    {
        var comments = await _commentRepository.GetCommentsAsync(request.PostId, request.PageNumber);
        return comments;
    }
}
