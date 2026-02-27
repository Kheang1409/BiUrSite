using Backend.Domain.Posts;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Posts.GetMyPosts;

internal sealed class GetMyPostsQueryHandler : IRequestHandler<GetMyPostsQuery, IEnumerable<Post>>
{
    private readonly IPostRepository _postRepository;
    public GetMyPostsQueryHandler(
        IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IEnumerable<Post>> Handle(GetMyPostsQuery request, CancellationToken cancellationToken)
    {
        return await _postRepository.GetPosts(new UserId(request.UserId), string.Empty, request.PageNumber);;
    }
}