using Backend.Domain.Enums;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Users.Admin;

internal sealed class BanUserCommandHandler : IRequestHandler<BanUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BanUserCommandHandler> _logger;

    public BanUserCommandHandler(
        IUserRepository userRepository,
        ILogger<BanUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        

        var user = await _userRepository.GetUserById(new UserId(request.UserId));

        if (user is null)
            throw new NotFoundException("User not found.");

        if (user.Status == Status.Deleted)
            throw new ConflictException("Cannot ban a deleted user.");

        if (user.Status == Status.Banned)
            throw new ConflictException("User is already banned.");

        if (user.Role == Role.Admin)
            throw new ForbiddenException("Cannot ban an admin user.");

        var duration = request.DurationMinutes.HasValue
            ? TimeSpan.FromMinutes(request.DurationMinutes.Value)
            : (TimeSpan?)null;

        user.Ban(request.Reason, duration);
        await _userRepository.Update(user);

        
    }
}
