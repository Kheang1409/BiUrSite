using Backend.Domain.Posts.Interfaces;
using MediatR;

namespace Backend.Application.Features.Posts.CountUserTotalPost;
public class CountUserTotalPostCommandHandler : IRequestHandler<CountUserTotalPostCommand, int>
{
    private readonly IPostRepository _postRepository;

    public CountUserTotalPostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<int> Handle(CountUserTotalPostCommand request, CancellationToken cancellationToken)
        => await _postRepository.GetUserPostCountAsync(request.UserId);
}
