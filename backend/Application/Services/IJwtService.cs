using Backend.Domain.Users.Entities;

namespace Backend.Application.Services;
public interface IJwtService
{
    string GenerateToken(User user);
}
