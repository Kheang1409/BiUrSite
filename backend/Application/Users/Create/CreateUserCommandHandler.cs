using Backend.Application.Data;
using Backend.Domain.Enums;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Users.CreateUser;

internal sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, User>
{
    private readonly IUserRepository _userRepository;
    private readonly IEnumerable<IUserFactory> _factories;
    private readonly IUnitOfWork _unitOfWord;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IEnumerable<IUserFactory> factories,
        IUnitOfWork unitOfWord)
    {
        _userRepository = userRepository;
        _factories = factories;
        _unitOfWord = unitOfWord;
    }

    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user is not null && user.Status == Status.Active) //Existing User & Active
            throw new ConflictException("Email is already registered.");

        if (user is null) // Not Exisiting in DB
        {
            var factory = _factories.OfType<UserFactory>().First();
            user = factory.Create(null, request.Username, request.Email, request.Password, string.Empty);
            _userRepository.Create(user);
        }
        else if (user.Status != Status.Active)  // Exisiting in DB & Inactive
        {
            user.ResetUsername(request.Username)
                .ResetPassword(request.Password)
                .ReCreate();
        }

        await _unitOfWord.SaveChangesAsync(cancellationToken);
        return user;
    }
}
