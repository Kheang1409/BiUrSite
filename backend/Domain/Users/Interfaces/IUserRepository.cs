using Backend.Domain.Users.Entities;

namespace Backend.Domain.Users.Interfaces;
public interface IUserRepository
{
    Task<List<User>> GetUsersAsync(int pageNumber, string? usernamme);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByOtpAsync(string otp);
    Task<bool> VerifyUserAsync(string verifiedToken);
    Task<bool> RequestPasswordResetAsync(string email, string? otp);
    Task<bool> ResetUserPasswordAsync(string otp, string hashPassword);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task<bool> BanUserAsync(int userId);
    Task<bool> SoftDeleteUserAsync(int userId);
    Task<bool> DeleteUserAsync(int userId);
}
