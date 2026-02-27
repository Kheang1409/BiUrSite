using Backend.Domain.Enums;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.UpdateProfileNotificationStatus;

internal sealed class UpdateProfileNotificationStatusCommandHandler : IRequestHandler<UpdateProfileNotificationStatusCommand>
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
        if (user is not {Status: Status.Active} )
            throw new UnauthorizedAccessException($"User is {user!.Status}.");
        user.MarkNotificationsAsRead();
        await _userRepository.Update(user);
    }
}
