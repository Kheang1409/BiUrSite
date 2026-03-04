using MediatR;

namespace Backend.Application.Users.Admin;

public record UnbanUserCommand(Guid UserId) : IRequest;
