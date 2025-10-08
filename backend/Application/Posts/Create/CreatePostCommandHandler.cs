using Backend.Application.Data;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Posts.Create;

internal sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Post>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostFactory _postFactory;
    private readonly IUnitOfWork _unitOfWord;

    public CreatePostCommandHandler(
        IPostRepository postRepository,
        IPostFactory postFactory,
        IUnitOfWork unitOfWord)
    {
        _postRepository = postRepository;
        _postFactory = postFactory;
        _unitOfWord = unitOfWord;
    }

    public async Task<Post> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = _postFactory.Create(
            new UserId(request.UserId),
            request.Username,
            request.Text,
            request.Data
        );
        await Task.WhenAll(_postRepository.Create(post), _unitOfWord.SaveChangesAsync(post, cancellationToken));
        return post;
    }
}