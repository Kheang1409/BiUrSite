using Backend.Application.Data;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Users.ResetPassword;

internal sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, User?>
{
    private readonly IUserRepository _userRepository;
     private readonly IUnitOfWork _unitOfWork;
    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<User?> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailWithOtp(request.Email, request.Otp);
        if (user is null)
            throw new NotFoundException("Invalid or expired OTP.");
        if(user.Status != Domain.Enums.Status.Active)
            throw new UnauthorizedAccessException($"User is {user.Status}.");
        user.ResetPassword(request.Password);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user;
    }
}