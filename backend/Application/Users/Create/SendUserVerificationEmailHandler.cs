using Backend.Application.Configuration;
using Backend.Application.Messaging.Emails;
using Backend.Application.Services;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;
using Rebus.Handlers;

namespace Backend.Application.Users.Create;

internal sealed class SendUserVerificationEmailHandler : IHandleMessages<UserCreatedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IAppOptions _options;
    private const string SUBJECT = "ACCOUNT VERIFICATION";

    public SendUserVerificationEmailHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        IAppOptions options)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _options = options;
    }

    public async Task Handle(UserCreatedEvent message)
    {
        var user = await _userRepository.GetUserById(new UserId(message.Id));
        if (user is { Token: null})
            return;
        var encodedToken = Uri.EscapeDataString(user!.Token.Value);
        var verificationUrl = $"{_options.BaseUrl}/api/users/verify?token={encodedToken}";
        var email = VerificationEmail.Create(user.Email, SUBJECT, user.Username, verificationUrl);
        await _emailService.Send(email);
    }
}
