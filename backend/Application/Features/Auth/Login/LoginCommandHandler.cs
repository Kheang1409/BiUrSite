using Backend.Application.Services;
using Backend.Domain.Users.Interfaces;
using MediatR;

namespace Backend.Application.Features.Auth.Login;


public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IUserRepository userRepository, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
    }

    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user == null || user.VerifyPassword(request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");
        var token = _tokenService.CreateToken(user.Id, user.Email, user.Username);
        return token;
    }
}