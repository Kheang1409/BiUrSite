using Backend.Application.Storage;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using Rebus.Handlers;

namespace Backend.Application.Posts.Create;

internal sealed class UploadImageHandler : IHandleMessages<PostCreatedEvent>
{
    private readonly IPostRepository _postRepository;
    private readonly IImageStorageService _imageStorageService;
    public UploadImageHandler(
        IPostRepository postRepository,
        IImageStorageService imageStorageService)
    {
        _postRepository = postRepository;
        _imageStorageService = imageStorageService;
    }

    public async Task Handle(PostCreatedEvent message)
    {
        var post = await _postRepository.GetPostById(new PostId(message.Id));
        if (post is null)
            throw new NotFoundException("Post is not found.");
        if (message.Data is not null && message.Data.Length > 0)
            {
                var id = post.Id.Value.ToString();
                var fileName = $"posts/{id}.jpg";
                var url = await _imageStorageService.UploadImageAsync(fileName, message.Data);
                post.SetImage(url);
            }
        await _postRepository.Update(post);
    }
}
