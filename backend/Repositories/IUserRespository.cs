using Backend.Models;

namespace Backend.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsersAsync(int pageNumber, string? usernamme);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UserVerifiedAsync(string verifiedToken);
        Task<bool> UserForgetPasswordAsync(string email, string? otp);
        Task<bool> UserResetPasswordAsync(string otp, string hashPassword);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> BanUserAsync(int userId);
        Task<bool> SoftDeleteUserAsync(int userId);
        Task<bool>  DeleteUserAsync(int userId);
    }
}
