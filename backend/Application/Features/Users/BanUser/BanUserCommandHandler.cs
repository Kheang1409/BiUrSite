using Backend.Domain.Common.Enums;
using Backend.Domain.Users.Interfaces;
using MediatR;

namespace Backend.Application.Features.Users.BanUser;

public class BanUserCommandHandler : IRequestHandler<BanUserCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public BanUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(BanUserCommand request, CancellationToken cancellationToken){
        var user = await _userRepository.GetUserByIdAsync(request.Id);
        if(user == null)
            throw new Exception("User not found");
        if(user.Status == Status.Banned)
            throw new Exception("User already banned");
        await _userRepository.BanUserAsync(request.Id);
        return true;
    }
}