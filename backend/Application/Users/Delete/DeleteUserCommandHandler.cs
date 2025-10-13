using Backend.Application.Data;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Users.Delete;

internal sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserById(new UserId(request.Id));
        if (user is null)
            throw new NotFoundException("User not found.");
        if (user.Status != Domain.Enums.Status.Active)
            throw new UnauthorizedAccessException($"User is {user.Status}.");
        user.Delete();
        await _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(user, cancellationToken);
    }
}