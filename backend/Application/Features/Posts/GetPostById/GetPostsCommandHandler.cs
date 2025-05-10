using Backend.Domain.Posts.Entities;
using Backend.Domain.Posts.Interfaces;
using MediatR;

namespace Backend.Application.Features.Posts.GetPostById;
public class GetPostByIdCommandHandler : IRequestHandler<GetPostByIdCommand, Post?>
{
    private readonly IPostRepository _postRepository;

    public GetPostByIdCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Post?> Handle(GetPostByIdCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostByIdAsync(request.PostId);
        return post;
    }
}
