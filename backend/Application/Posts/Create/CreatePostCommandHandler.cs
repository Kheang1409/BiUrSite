using Backend.Application.Data;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Posts.Create;

internal sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Post>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostFactory _postFactory;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePostCommandHandler(
        IPostRepository postRepository,
        IPostFactory postFactory,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _postFactory = postFactory;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Post> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetUserById(userId);

        if (user is null)
            throw new NotFoundException("User not found.");

        var post = _postFactory.Create(
            userId,
            request.Username,
            request.Text,
            request.Data
        );

        post.SetUser(user);
        await _postRepository.Create(post);
        await _unitOfWork.SaveChangesAsync(post, cancellationToken);
        return post;
    }
}