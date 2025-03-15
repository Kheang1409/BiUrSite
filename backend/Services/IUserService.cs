using Backend.Models;
namespace Backend.Services
{
    public interface IUserService
    {

        Task<List<User>> GetUsersAsync(int pageNumber, string? username);
        Task<User> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task<bool> UserVerifiedAsync(string verifiedToken);
        Task<bool> UserForgetPasswordAsync(string email, string? otp);
        Task<bool> UserResetPasswordAsync(string otp, string hashPassword);
        Task UpdateUserAsync(User user);
        Task<bool> BanUserAsync(int userId);
        Task<bool> SoftDeleteUserAsync(int userId);
        Task<bool> DeleteUserAsync(int userId);
    }
}