using Backend.Application.Storage;
using Backend.Domain.Posts;
using MongoDB.Bson;
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
        var post = await _postRepository.GetPostById(message.Id);
        if (message.Data is not null && message.Data.Length > 0)
        {
            var id = ObjectId.GenerateNewId().ToString();
            var fileName = $"posts/{id}.jpg";   
            var url = await _imageStorageService.UploadImageAsync(fileName, message.Data);
            post.SetImage(id, url);
        }
        await _postRepository.Update(post);
    }
}
