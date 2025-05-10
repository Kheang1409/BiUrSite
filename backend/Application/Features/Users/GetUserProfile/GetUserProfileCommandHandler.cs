using Backend.Domain.Users.Entities;
using Backend.Domain.Users.Interfaces;
using MediatR;

namespace Backend.Application.Features.Users.GetUserProfile;

public class GetUserProfileCommandHandler : IRequestHandler<GetUserProfileCommand, User?>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> Handle(GetUserProfileCommand request, CancellationToken cancellationToken){
        return await _userRepository.GetUserByEmailAsync(request.Email);
    }
}