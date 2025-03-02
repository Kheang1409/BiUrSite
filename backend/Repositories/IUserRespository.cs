using Backend.Models;

namespace Backend.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsersAsync(int pageNumber, string usernamme);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByOPTAsync(string opt);
        Task<User?> GetUserByVerificationTokenAsync(string verifiedToken);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
    }
}
