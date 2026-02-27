using Backend.Application.Storage;
using Backend.Domain.Enums;
using Backend.Domain.Images;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.Update;

internal sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IImageStorageService _imageStorageService;
    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        IImageStorageService imageStorageService
    )
    {
        _userRepository = userRepository;
        _imageStorageService = imageStorageService;
    }
    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user is null)
            throw new UnauthorizedAccessException("User not found.");
        if (user.Status != Status.Active)
            throw new UnauthorizedAccessException($"User is {user.Status}.");
        var fileName = $"profiles/{user.Id.Value}.jpg";
        var profileImage = user.Profile;

        var hasNewImage = request.Data is { Length: > 0 };

        if (hasNewImage)
        {
            var url = await _imageStorageService.UploadImageAsync(fileName, request.Data!);
            profileImage = new Image(url);
        }

        if (request.RemoveImage && !hasNewImage)
        {
            await _imageStorageService.DeleteImageAsync(fileName);
            profileImage = new Image(User.DefaultProfileUrl);
        }

        user.Update(request.Username, request.Bio ?? string.Empty, profileImage, request.Phone);
        await _userRepository.Update(user);
    }
}
