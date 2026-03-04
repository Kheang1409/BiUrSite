using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.Admin;

public record GetAllUsersQuery(int PageNumber) : IRequest<IEnumerable<User>>;
