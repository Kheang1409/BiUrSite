using MediatR;

namespace Backend.Application.Features.Users.BanUser;
public record BanUserCommand(int Id) : IRequest<bool>;