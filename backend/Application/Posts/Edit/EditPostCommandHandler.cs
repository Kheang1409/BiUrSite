using Backend.Application.Data;
using Backend.Application.Storage;
using Backend.Domain.Posts;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Posts.Edit;

internal sealed class EditPostCommandHandler : IRequestHandler<EditPostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorageService _imageStorageService;
    public EditPostCommandHandler(
        IPostRepository postRepository,
        IUnitOfWork unitOfWork,
        IImageStorageService imageStorageService
    )
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _imageStorageService = imageStorageService;
    }
    public async Task Handle(EditPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostById(new PostId(request.Id));
        if (post is null)
            throw new NotFoundException("Post is not found.");
        if(!post.UserId.Value.Equals(request.UserId))
            throw new ForbiddenException("You are not authorized to edit this post.");

        var hasNewImage = request.Data is { Length: > 0 };
        var wantsRemoveImage = request.RemoveImage;
        var hasExistingImage = post.Image is not null && !string.IsNullOrEmpty(post.Image.Url);
        var hasTextChange = request.Text != post.Text;

        if (!hasTextChange && !hasNewImage && !(wantsRemoveImage && hasExistingImage))
            return;

        if (hasTextChange)
            post.UpdateContent(request.Text);

        if (hasNewImage)
        {
            var id = post.Id.Value.ToString();
            var fileName = $"posts/{id}.jpg";
            var url = await _imageStorageService.UploadImageAsync(fileName, request.Data!);
            post.SetImage(url);
        }
        else if (wantsRemoveImage)
        {
            if (hasExistingImage)
            {
                var id = post.Id.Value.ToString();
                var fileName = $"posts/{id}.jpg";
                await _imageStorageService.DeleteImageAsync(fileName);
            }
            post.RemoveImage();
        }

        await _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(post, cancellationToken);
    }
}
