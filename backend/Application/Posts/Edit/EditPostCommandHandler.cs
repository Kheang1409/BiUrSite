using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Posts.Edit;

public record EditPostCommandHandler : IRequestHandler<EditPostCommand>
{
    private readonly IPostRepository _postRepository;
    public EditPostCommandHandler(
        IPostRepository postRepository
    )
    {
        _postRepository = postRepository;
    }
    public async Task Handle(EditPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostById(new PostId(request.Id));
        if (post is null)
            throw new NotFoundException("Post is not found.");
        if(!post.UserId.Value.Equals(request.UserId))
            throw new ForbiddenException("You are not authorized to edit this post.");
        post.UpdateContent(request.Text);
        await _postRepository.Update(post);
    }
}
