using Backend.Domain.Services;
using Backend.Domain.Users.Entities;
using Backend.Domain.Users.Interfaces;
using MediatR;

namespace Backend.Application.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public ResetPasswordCommandHandler(IUserRepository userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        => await _userRepository.ResetUserPasswordAsync(request.Opt, User.HashPassword(request.NewPassword));
}