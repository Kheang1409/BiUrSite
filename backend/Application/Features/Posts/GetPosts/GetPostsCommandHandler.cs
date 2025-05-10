using Backend.Domain.Posts.Entities;
using Backend.Domain.Posts.Interfaces;
using MediatR;

namespace Backend.Application.Features.Posts.GetPosts;
public class GetPostsCommandHandler : IRequestHandler<GetPostsCommand, List<Post>>
{
    private readonly IPostRepository _postRepository;

    public GetPostsCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<List<Post>> Handle(GetPostsCommand request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.SearchPostsAsync(request.Keyword, request.PageNumber);
        return posts;
    }
}
