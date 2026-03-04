using Backend.Application.Messaging.Emails;
using Backend.Application.Services;
using Backend.Domain.Users;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Backend.Application.Users.ForgotPassword;

internal sealed class SendUserOTPEmailHandler : IHandleMessages<UserForgotPasswordEvent>
{

    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendUserOTPEmailHandler>? _logger;
    private const string SUBJECT = "RESET PASSWORD";

    public SendUserOTPEmailHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<SendUserOTPEmailHandler>? logger = null)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(UserForgotPasswordEvent message)
    {
        var user = await _userRepository.GetUserById(new UserId(message.Id));
        if (user is null)
        {
            _logger?.LogWarning("SendUserOTPEmailHandler: user {UserId} not found.", message.Id);
            return;
        }

        var otpValue = user.Otp?.Value;
        if (string.IsNullOrWhiteSpace(otpValue))
        {
            _logger?.LogWarning("SendUserOTPEmailHandler: user {UserId} has no OTP. Skipping email.", message.Id);
            return;
        }

        try
        {
            var email = PasswordResetEmail.Create(user.Email, SUBJECT, user.Username, otpValue);
            await _emailService.Send(email);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send password reset email to user {UserId} ({Email}).", message.Id, user.Email);
        }
    }
}
