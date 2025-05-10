using Backend.Domain.Posts.Interfaces;
using MediatR;

namespace Backend.Application.Features.Posts.UpdatePost;
public class UpdatePostWithUserIdCommandHandler : IRequestHandler<UpdatePostWithPostIdCommand, bool>
{
    private readonly IPostRepository _postRepository;

    public UpdatePostWithUserIdCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<bool> Handle(UpdatePostWithPostIdCommand request, CancellationToken cancellationToken)
    {
        return await _postRepository.UpdatePostDescriptionAsync(request.PostId, request.Description);
    }
}
