using Backend.Domain.Posts;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Posts.GetPosts;

internal sealed class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, IEnumerable<Post>>
{
    private readonly IPostRepository _postRepository;
    public GetPostsQueryHandler(
        IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IEnumerable<Post>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        return await _postRepository.GetPosts(null, request.Keywords, request.PageNumber);;
    }
}