using Backend.Application.Data;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Posts.Delete;

internal sealed class DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;


    public DeletePostCommandHandler(
        IPostRepository postRepository,
        IUnitOfWork unitOfWork
    )
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostById(new PostId(request.Id));
        if (post is null)
            throw new NotFoundException("Post is not found.");
        if (!post.UserId.Value.Equals(request.UserId))
            throw new ForbiddenException("You are not authorized to edit this post.");
        post.Delete();
        await _postRepository.Delete(post);
        await _unitOfWork.SaveChangesAsync(post, cancellationToken);
    }
}
