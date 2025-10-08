using Backend.Domain.Comments;
using Backend.Domain.Posts;
using MediatR;

namespace Backend.Application.Comments.GetComment;

internal sealed class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, Comment?>
{
    private readonly ICommentRepository _commentRepository;
    public GetCommentByIdQueryHandler(
        ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Comment?> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _commentRepository.GetCommentById(request.PostId, request.Id);;
    }
}