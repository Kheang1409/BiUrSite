using Backend.Domain.Enums;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Users.UpdateProfileNotificationStatus;

public record UpdateProfileNotificationStatusCommandHandler : IRequestHandler<UpdateProfileNotificationStatusCommand>
{
    private readonly IUserRepository _userRepository;
    public UpdateProfileNotificationStatusCommandHandler(
        IUserRepository userRepository
    )
    {
        _userRepository = userRepository;
    }
    public async Task Handle(UpdateProfileNotificationStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user is null)
            throw new NotFoundException("User is not found.");
        if (user.Status != Status.Active)
            throw new UnauthorizedAccessException($"User is {user.Status}.");
        user.MarkNotificationsAsRead();
        await _userRepository.Update(user);
    }
}
