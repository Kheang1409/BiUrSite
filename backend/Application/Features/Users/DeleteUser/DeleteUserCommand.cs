using MediatR;

namespace Backend.Application.Features.Users.DeleteUser;
public record DeleteUserCommand(int Id) : IRequest<bool>;