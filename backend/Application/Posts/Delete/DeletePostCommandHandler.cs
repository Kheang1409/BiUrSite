using Backend.Application.Data;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Posts.Delete;

public record DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWord;


    public DeletePostCommandHandler(
        IPostRepository postRepository,
        IUnitOfWork unitOfWord
    )
    {
        _postRepository = postRepository;
        _unitOfWord = unitOfWord;
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
        await _unitOfWord.SaveChangesAsync(post, cancellationToken);
    }
}
