using Backend.Application.Data;
using Backend.Domain.Enums;
using Backend.Domain.Users;
using Backend.SharedKernel.Exceptions;
using MediatR;

namespace Backend.Application.Users.Update;

public record UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork
    )
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user is null)
            throw new NotFoundException("User is not found.");
        if (user.Status != Status.Active)
            throw new UnauthorizedAccessException($"User is {user.Status}.");
        user.Update(request.Username, request.Bio);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
