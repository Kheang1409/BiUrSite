using Backend.Application.Data;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.CreateByOAuth;

internal sealed class CreateUserByOAuthCommandHandler : IRequestHandler<CreateUserByOAuthCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEnumerable<IUserFactory> _factories;
    private readonly IUnitOfWork _unitOfWord;
    public CreateUserByOAuthCommandHandler(
        IUserRepository userRepository,
        IEnumerable<IUserFactory> factories,
        IUnitOfWork unitOfWord)
    {
        _userRepository = userRepository;
        _factories = factories;
        _unitOfWord = unitOfWord;
    }

    public async Task Handle(CreateUserByOAuthCommand request, CancellationToken cancellationToken)
    {
        var existUser = await _userRepository.GetUserByEmail(request.Email);
        if (existUser != null)
        {
            return;
        }
        var factory = _factories.OfType<OAuthUserFactory>().First();
        var user = factory.Create(new UserId(request.Id), request.Username, request.Email, "", request.AuthProvider);
        await _userRepository.Create(user);
        await _unitOfWord.SaveChangesAsync(user, cancellationToken);
    }
}
