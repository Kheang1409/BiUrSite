using Backend.Domain.Comments;
using Backend.Domain.Posts;
using MediatR;

namespace Backend.Application.Comments.GetComments;

internal sealed class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, IEnumerable<Comment>>
{
    private readonly ICommentRepository _commentRepository;
    public GetCommentsQueryHandler(
        ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<Comment>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        return await _commentRepository.GetComments(request.PostId, request.PageNumber);;
    }
}