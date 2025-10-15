using Backend.Application.Data;
using Backend.Application.Storage;
using Backend.Domain.Enums;
using Backend.Domain.Images;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Users.Update;

public record UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
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
            throw new NotFoundException("User is not found.");
        if (user.Status != Status.Active)
            throw new UnauthorizedAccessException($"User is {user.Status}.");
        var fileName = $"profiles/{user.Id.Value}.jpg";
        var profileImage = user.Profile;
        if (request.Data is not null && request.Data.Length > 0)
        {
            var url = await _imageStorageService.UploadImageAsync(fileName, request.Data);
            profileImage = new Image(url);
        }
        user.Update(request.Username, request.Bio ?? string.Empty, profileImage);
        await _userRepository.Update(user);
    }
}
