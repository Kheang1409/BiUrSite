using Backend.Enums;
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
                users  = users.Where(user=> user.username.ToLower().Contains(username.ToLower()));
            var userList = await users.ToListAsync();
            return userList;
        }

        public async Task<User> GetUserByIdAsync(int userId) =>
            await _context.Users.FindAsync(userId);

        public async Task<User> GetUserByEmailAsync(string email) =>
            await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(user => user.email == email && user.status == Status.Verified);
        

        public async Task<bool> UserVerified(string verificationToken){
            int affectdRow = await _context.Users.Where(user => user.verificationToken.Equals(verificationToken) && user.status == Status.Unverified)
            .ExecuteUpdateAsync(user => user
                .SetProperty(user => user.status, Status.Verified)
                .SetProperty(user => user.verificationToken, string.Empty)
                .SetProperty(user => user.verificationTokenExpiry, (DateTime?)null));
            return affectdRow == 1; 
        }

        public async Task<bool> UserForgetPassword(string email, string opt){
            int affectdRow = await _context.Users.Where(user => user.email.Equals(email) && user.status == Status.Verified)
            .ExecuteUpdateAsync(user => user
                .SetProperty(user => user.opt, opt)
                .SetProperty(user => user.optExpiry, DateTime.UtcNow.AddMinutes(3)));
            return affectdRow == 1;
        }
        public async Task<bool>  UserResetPassword(string opt, string hashPassword){
            int affectdRow = await _context.Users.Where(user => user.opt.Equals(opt) && user.status == Status.Verified)
            .ExecuteUpdateAsync(user => user
                .SetProperty(user => user.password, hashPassword)
                .SetProperty(user => user.opt, string.Empty)
                .SetProperty(user => user.optExpiry, (DateTime?)null));
            return affectdRow == 1;
        }
        public async Task AddUserAsync(User user) {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user){
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> BanUserAsync(int userId){
            int affectdRow = await _context.Users.Where(user => user.userId == userId)
            .ExecuteUpdateAsync(user => user
                .SetProperty(user => user.status, Status.Banned));
            return affectdRow == 1;
        }

        public async Task DeleteUserAsync(int userId)=>
           await _context.Users.Where(user=> user.userId == userId)
           .ExecuteDeleteAsync();
    }
}
