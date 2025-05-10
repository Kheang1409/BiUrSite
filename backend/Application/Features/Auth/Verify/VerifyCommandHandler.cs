using Backend.Domain.Users.Interfaces;
using MediatR;

namespace Backend.Application.Features.Auth.Verify;


public class VerifyCommandHandler : IRequestHandler<VerifyCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public VerifyCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(VerifyCommand request, CancellationToken cancellationToken)
    {
        return await _userRepository.VerifyUserAsync(request.Token);
    }
}