using Backend.Application.Data;
using Backend.Domain.Enums;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Users.VerifyUser;

internal sealed class VerifyUserCommandHandler : IRequestHandler<VerifyUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(VerifyUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByToken(request.Token);
        if (user is null)
            throw new NotFoundException("Invalid or expired token.");
        if (user.Status != Status.Unverified)
            return;
        user.Verify();
        await _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(user, cancellationToken);
    }
}
