using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.Delete;

public record DeleteUserCommand(Guid Id) : IRequest;
