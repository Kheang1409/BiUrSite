using Backend.Application.Data;
using Backend.Domain.Enums;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.ForgotPassword;

internal sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;

    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user is not {Status: Status.Active})
            throw new UnauthorizedAccessException($"User is {user?.Status}.");
        user.ForgotPassword();
        await _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(user, cancellationToken);

    }
}