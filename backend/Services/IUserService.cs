using Backend.DTOs;
using Backend.Models;
namespace Backend.Services
{
    public interface IUserService
    {

        Task<List<UserDto>> GetUsersAsync(int pageNumber, string username);
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByOPTAsync(string opt);
        Task<User> GetUserByVerificationTokenAsync(string verifiedToken);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
    }
}