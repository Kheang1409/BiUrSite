using Backend.Domain.Enums;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Users.Admin;

internal sealed class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UnbanUserCommandHandler> _logger;

    public UnbanUserCommandHandler(
        IUserRepository userRepository,
        ILogger<UnbanUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Handle(UnbanUserCommand request, CancellationToken cancellationToken)
    {
        

        var user = await _userRepository.GetUserById(new UserId(request.UserId));

        if (user is null)
            throw new NotFoundException("User not found.");

        if (user.Status != Status.Banned)
            throw new ConflictException("User is not banned.");

        user.Unban();
        await _userRepository.Update(user);

        
    }
}
