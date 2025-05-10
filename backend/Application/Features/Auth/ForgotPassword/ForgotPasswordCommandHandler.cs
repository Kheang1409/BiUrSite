using Backend.Domain.Services;
using Backend.Domain.Users.Interfaces;
using MediatR;

namespace Backend.Application.Features.Auth.ForgotPassword;


public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUserRepository userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        user.GenerateOtp();
        await Task.WhenAll(
            _userRepository.RequestPasswordResetAsync(user.Email, user.Otp),
            _emailService.SendOtpEmail(user.Email, user.Otp));
        return Unit.Value;
    }
}