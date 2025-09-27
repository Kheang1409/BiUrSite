namespace Backend.Application.Services;
public interface ITokenService
{
    string GenerateToken(Guid userId, string email, string username, string role);
}