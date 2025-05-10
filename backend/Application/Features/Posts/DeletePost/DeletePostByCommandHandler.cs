using Backend.Domain.Posts.Interfaces;
using MediatR;

namespace Backend.Application.Features.Posts.DeletePost;
public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, bool>
{
    private readonly IPostRepository _postRepository;

    public DeletePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        return await _postRepository.SoftDeletePostAsync(request.PostId);
    }
}
