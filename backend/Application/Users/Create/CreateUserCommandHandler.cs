using Backend.Application.Data;
using Backend.Domain.Enums;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Users.Create;

internal sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, User>
{
    private readonly IUserRepository _userRepository;
    private readonly IStandardUserFactory _userFactory;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IStandardUserFactory userFactory,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userFactory = userFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user is { Status: Status.Active })
            throw new ConflictException("Email is already registered.");

        if (user is null)
        {
            user = _userFactory.Create(null, request.Username, request.Email, request.Password, string.Empty);
            await _userRepository.Create(user);
        }
        else
        {
            user.ResetUsername(request.Username)
                .ResetPassword(request.Password)
                .ReCreate();
            await _userRepository.Update(user);
        }

        if (string.Equals(Environment.GetEnvironmentVariable("ENABLE_REQUEST_TRACING"), "1"))
        {
            try
            {
                user.Verify();
                await _userRepository.Update(user);
            }
            catch { }
        }

        await _unitOfWork.SaveChangesAsync(user, cancellationToken);
        return user;
    }
}
