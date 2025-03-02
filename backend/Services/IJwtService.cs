namespace Backend.Services
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string userName, string role);
    }
}
