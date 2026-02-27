using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.GetUser;

public record GetUserByIdQuery(Guid Id) : IRequest<User?>;
