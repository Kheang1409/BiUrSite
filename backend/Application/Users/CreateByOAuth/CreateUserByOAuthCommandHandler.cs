using Backend.Application.Data;
using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.CreateByOAuth;

internal sealed class CreateUserByOAuthCommandHandler : IRequestHandler<CreateUserByOAuthCommand, User>
{
    private readonly IUserRepository _userRepository;
    private readonly IOAuthUserFactory _userFactory;
    private readonly IUnitOfWork _unitOfWork;
    public CreateUserByOAuthCommandHandler(
        IUserRepository userRepository,
        IOAuthUserFactory userFactory,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userFactory = userFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<User> Handle(CreateUserByOAuthCommand request, CancellationToken cancellationToken)
    {
        var existedUser = await _userRepository.GetUserByEmail(request.Email);
        if (existedUser != null)
        {
            return existedUser;
        }
        var user = _userFactory.Create(new UserId(request.Id), request.Username, request.Email, null, request.AuthProvider);
        await _userRepository.Create(user);
        await _unitOfWork.SaveChangesAsync(user, cancellationToken);
        return user;
    }
}
