using Backend.Enums;

namespace Backend.Services
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string userName, Role role);
    }
}
