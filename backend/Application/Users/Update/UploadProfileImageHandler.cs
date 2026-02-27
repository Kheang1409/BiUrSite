using Backend.Application.Storage;
using Backend.Domain.Enums;
using Backend.Domain.Users;
using Rebus.Handlers;

namespace Backend.Application.Users.Update;

internal sealed class UploadProfileImageHandler : IHandleMessages<UpdatedProfileEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IImageStorageService _imageStorageService;
    public UploadProfileImageHandler(
        IUserRepository userRepository,
        IImageStorageService imageStorageService)
    {
        _userRepository = userRepository;
        _imageStorageService = imageStorageService;
    }

    public async Task Handle(UpdatedProfileEvent message)
    {
        var user = await _userRepository.GetUserById(new UserId(message.Id));
        if (user is not {Status: Status.Active})
            throw new UnauthorizedAccessException($"User is {user!.Status}.");
        if (message.Data is not { Length: 0})
            {
                var id = user.Id.Value.ToString();
                var fileName = $"profiles/{id}.jpg";
                var url = await _imageStorageService.UploadImageAsync(fileName, message.Data!);
            }
        await _userRepository.Update(user);
    }
}
