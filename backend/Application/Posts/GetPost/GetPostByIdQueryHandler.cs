using Backend.Domain.Posts;
using MediatR;

namespace Backend.Application.Posts.GetPost;

internal sealed class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, Post?>
{
    private readonly IPostRepository _postRepository;
    public GetPostByIdQueryHandler(
        IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Post?> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        return await _postRepository.GetPostById(new PostId(request.Id));;
    }
}