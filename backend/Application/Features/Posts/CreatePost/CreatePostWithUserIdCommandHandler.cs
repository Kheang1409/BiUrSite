using Backend.Domain.Posts.Entities;
using Backend.Domain.Posts.Interfaces;
using MediatR;

namespace Backend.Application.Features.Posts.CreatePost;
public class CreatePostWithUserIdCommandHandler : IRequestHandler<CreatePostWithUserIdCommand, Post>
{
    private readonly IPostRepository _postRepository;

    public CreatePostWithUserIdCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Post> Handle(CreatePostWithUserIdCommand request, CancellationToken cancellationToken)
    {
        var post = Post.Create(request.UserId, request.Description);
        post = await _postRepository.CreatePostAsync(post);
        if(post == null)
            throw new Exception("unable to comment!");
        return post;
    }
}
