using Backend.Domain.Users;
using MediatR;

namespace Backend.Application.Users.GetUsers;

public record GetUsersQuery(
    int PageNumber=1
) : IRequest<IEnumerable<User>>;
