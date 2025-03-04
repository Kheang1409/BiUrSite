using Backend.DTOs;
using Backend.Models;
namespace Backend.Services
{
    public interface IUserService
    {

        Task<List<UserDto>> GetUsersAsync(int pageNumber, string username);
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task<bool> UserVerified(string verifiedToken);
        Task<bool> UserForgetPassword(string email, string opt);
        Task<bool> UserResetPassword(string opt, string hashPassword);
        Task UpdateUserAsync(User user);
        Task<bool> BanUserAsync(int userId);
        Task DeleteUserAsync(int userId);
    }
}