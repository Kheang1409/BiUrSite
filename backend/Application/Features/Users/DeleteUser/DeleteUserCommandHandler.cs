using Backend.Domain.Common.Enums;
using Backend.Domain.Users.Interfaces;
using MediatR;

namespace Backend.Application.Features.Users.DeleteUser;

public class BanUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public BanUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken){
        var user = await _userRepository.GetUserByIdAsync(request.Id);
        if(user == null)
            throw new Exception("User not found");
        if(user.Status == Status.Banned)
            throw new Exception("User already banned");
        await _userRepository.SoftDeleteUserAsync(request.Id);
        return true;
    }
}