using Backend.Application.Data;
using Backend.Domain.Comments;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Comments.Create;

internal sealed class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Comment>
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCommentCommandHandler(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Comment> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var postId = new PostId(request.PostId);
        var userId = new UserId(request.UserId);
        var post = await _postRepository.GetPostById(postId);
        var user = await _userRepository.GetUserById(userId);

        if (post is null)
            throw new NotFoundException("Post not found.");

        if (user is null)
            throw new NotFoundException("User not found.");

        if (post is not { Status: Status.Active })
            throw new UnauthorizedAccessException($"Post is {post.Status}.");

        if (user is not { Status: Status.Active })
            throw new UnauthorizedAccessException($"User is {request.UserId}.");

        var comment = post.AddComment(user, request.Text);
        await _commentRepository.Create(postId, comment);
        await _unitOfWork.SaveChangesAsync(post, cancellationToken);
        return comment;
    }
}