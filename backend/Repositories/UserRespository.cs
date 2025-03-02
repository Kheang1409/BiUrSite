using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _limitItem;

        public UserRepository(AppDbContext context, IConfiguration configuration){
            _context = context;
            _configuration = configuration;
            _limitItem = int.Parse(_configuration["Limit"]);
        }

        public async Task<List<User>> GetUsersAsync(int pageNumber, string username){

            IQueryable<User> users = _context.Users.AsNoTracking()
            .Skip(_limitItem*pageNumber)
            .Take(_limitItem);
            if(username != null)
                users  = users.Where(u => u.username.ToLower().Contains(username.ToLower()));
            var userList = await users.ToListAsync();
            return userList;
        }

        public async Task<User> GetUserByIdAsync(int userId) =>
            await _context.Users.FindAsync(userId);

        public async Task<User> GetUserByEmailAsync(string email) =>
            await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.email == email && u.status == "Verified");
        public async Task<User?> GetUserByOPTAsync(string opt) =>
            await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.opt == opt && u.optExpiry > DateTime.UtcNow);

        public async Task<User?> GetUserByVerificationTokenAsync(string verificationToken) =>
            await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.verificationToken == verificationToken && u.verificationTokenExpiry > DateTime.UtcNow && u.status == "Unverified");
        public async Task AddUserAsync(User user) {
            _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user){
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)=>
           await _context.Users.Where(u => u.userId == userId)
           .ExecuteDeleteAsync();
    }
}
