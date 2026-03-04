namespace Backend.Domain.Users;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetUsers(int pageNumber);
    Task<IEnumerable<User>> GetAllUsers(int pageNumber);
    Task<User?> GetUserById(UserId id);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserByEmailWithOtp(string email, string otp);
    Task<User?> GetUserByToken(string token);
    Task Create(User user);
    Task Update(User user);
}