using Backend.Application.Data;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Posts.Admin;

internal sealed class AdminDeletePostCommandHandler : IRequestHandler<AdminDeletePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminDeletePostCommandHandler> _logger;

    public AdminDeletePostCommandHandler(
        IPostRepository postRepository,
        IUnitOfWork unitOfWork,
        ILogger<AdminDeletePostCommandHandler> logger)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AdminDeletePostCommand request, CancellationToken cancellationToken)
    {
        

        var post = await _postRepository.GetPostById(new PostId(request.PostId));

        if (post is null)
            throw new NotFoundException("Post not found.");

        if (post.Status == Status.Deleted)
            throw new ConflictException("Post is already deleted.");

        post.Delete();
        await _postRepository.Delete(post);
        await _unitOfWork.SaveChangesAsync(post, cancellationToken);

        
    }
}
