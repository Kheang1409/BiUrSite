namespace Backend.Application.Services
{
    public interface ITokenService
    {
        string CreateToken(int userId, string email, string userName);
        int TokenValidityInMinutes { get; }
    }
}
