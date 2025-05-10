using Backend.Domain.Users.Factories;
using Backend.Domain.Users.Interfaces;
using MediatR;

namespace Backend.Application.Features.Users.RegisterOAuthUser;

public class RegisterOAuthUserCommandHandler : IRequestHandler<RegisterOAuthUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserFactory _userFactory;

    public RegisterOAuthUserCommandHandler(IUserRepository userRepository, OAuthUserFactory userFactory)
    {
        _userRepository = userRepository;
        _userFactory = userFactory;
    }

    public async Task<Unit> Handle(RegisterOAuthUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            user = _userFactory.Create(request.Username, request.Email, request.Password);
            await _userRepository.CreateUserAsync(user);
        }

        return Unit.Value;
    }
}
