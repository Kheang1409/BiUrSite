using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Users.Login;

internal sealed class LoginCommandHandler : IRequestHandler<LoginCommand, User?>
{
    private readonly IUserRepository _userRepository;
    public LoginCommandHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user is null)
            throw new NotFoundException("User not found.");
        if(user.Status != Domain.Enums.Status.Active)
            throw new UnauthorizedAccessException($"User is {user.Status}.");
        if(!user.VerifyPassword(request.Password, user.Password)) 
            throw new UnauthorizedAccessException("Invalid username or password.");
        return user;
    }
}