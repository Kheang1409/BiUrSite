using Backend.Application.Storage;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MongoDB.Bson;
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
        if (user is null)
            throw new NotFoundException("User is not found.");
        if (message.Data is not null && message.Data.Length > 0)
            {
                var id = user.Id.Value.ToString();
                var fileName = $"profiles/{id}.jpg";
                var url = await _imageStorageService.UploadImageAsync(fileName, message.Data);
            }
        await _userRepository.Update(user);
    }
}
