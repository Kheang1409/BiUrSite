namespace Backend.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetUserById(UserId id);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserByEmailWithOtp(string email, string otp);
    Task<User?> GetUserByToken(string token);
    void Create(User user);
}