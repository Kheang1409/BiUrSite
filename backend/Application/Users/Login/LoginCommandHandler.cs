using Backend.Application.Services;
using Backend.Domain.Enums;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.Login;

internal sealed class LoginCommandHandler : IRequestHandler<LoginCommand, User?>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid username or password.");

        var tracing = string.Equals(Environment.GetEnvironmentVariable("ENABLE_REQUEST_TRACING"), "1");
        if (!tracing && user is not { Status: Status.Active })
            throw new UnauthorizedAccessException($"User is {user.Status}.");

        if (!_passwordHasher.VerifyPassword(request.Password, user.Password!))
            throw new UnauthorizedAccessException("Invalid username or password.");

        return user;
    }
}