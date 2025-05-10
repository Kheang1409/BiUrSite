using Backend.Domain.Users.Factories;
using Backend.Domain.Users.Interfaces;
using Microsoft.AspNetCore.Http;
using MediatR;
using Backend.Domain.Services;

namespace Backend.Application.Features.Users.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserFactory _userFactory;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RegisterUserCommandHandler(
        IUserRepository userRepository, 
        IUserFactory userFactory, 
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _userFactory = userFactory;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Unit> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
        if(existingUser is not null)
            throw new Exception("Email is already registered.");
        var user = _userFactory.Create(request.Username, request.Email, request.Password);
        var verificationLink = $"{_httpContextAccessor?.HttpContext?.Request.Scheme}://{_httpContextAccessor?.HttpContext?.Request.Host}/api/v1/users/verify?token={user.VerificationToken}";
        await Task.WhenAll(
            _userRepository.CreateUserAsync(user),
            _emailService.SendConfirmationEmail(user.Email, verificationLink)
        );
        return Unit.Value;
    }
}
