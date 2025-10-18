using Backend.Application.Data;
using Backend.Domain.Comments;
using Backend.Domain.Notifications;
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
    private readonly IUnitOfWork _unitOfWord;

    public CreateCommentCommandHandler(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWord)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _unitOfWord = unitOfWord;
    }

    public async Task<Comment> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var postId = new PostId(request.PostId);
        var post = await _postRepository.GetPostById(postId);
        if (post is null)
            throw new NotFoundException("Post not found.");
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetUserById(userId);
        if (user is null)
            throw new UnauthorizedAccessException($"User is {request.UserId}.");
        var comment = post.AddComment(user, request.Text);
        await _commentRepository.Create(postId, comment);
        await _unitOfWord.SaveChangesAsync(post, cancellationToken);
        return comment;
    }
}