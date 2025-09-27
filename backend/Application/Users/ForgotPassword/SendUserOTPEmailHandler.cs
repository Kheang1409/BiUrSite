using Backend.Application.Configuration;
using Backend.Application.Messaging.Emails;
using Backend.Application.Services;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Rebus.Handlers;

namespace Backend.Application.Users.ForgotPassword;

internal sealed class SendUserOTPEmailHandler : IHandleMessages<UserForgotPasswordEvent>
{

    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private const string SUBJECT = "RESET PASSWORD";

    public SendUserOTPEmailHandler(
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task Handle(UserForgotPasswordEvent message)
    {
        var user = await _userRepository.GetUserById(new UserId(message.Id));
        if (user is null)
            throw new NotFoundException("User not found.");

        var email = PasswordResetEmail.Create(user.Email, SUBJECT, user.Username, user.Otp!.Value);
        await _emailService.Send(email);
    }
}
