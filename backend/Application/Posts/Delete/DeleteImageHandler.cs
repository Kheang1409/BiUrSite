using Backend.Application.Storage;
using Rebus.Handlers;

namespace Backend.Application.Posts.Delete;

internal sealed class DeleteImageHandler : IHandleMessages<PostDeletedEvent>
{
    private readonly IImageStorageService _imageStorageService;
    public DeleteImageHandler(
        IImageStorageService imageStorageService)
    {
        _imageStorageService = imageStorageService;
    }

    public async Task Handle(PostDeletedEvent message)
    {
        if (message.Image is not null && !string.IsNullOrEmpty(message.Image.Url))
        {
            var id = message.Id.ToString();
            var fileName = $"posts/{id}.jpg";
            await _imageStorageService.DeleteImageAsync(fileName);
        }
    }
}
