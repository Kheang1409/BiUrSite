using Backend.Enums;
using Backend.Extensions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly int _limitItem;

        public UserRepository(AppDbContext context, IConfiguration ? configuration){
            _context = context;
            if(configuration != null)
                _limitItem = int.Parse(configuration["Limit"] ?? "5");
            else{
                _limitItem = 5;
            }
        }

        public async Task<List<User>> GetUsersAsync(int pageNumber, string? username){

            IQueryable<User> users = _context.Users.AsNoTracking()
                .Skip(_limitItem*pageNumber)
                .Take(_limitItem);
            if(username != null)
                users  = users.Where(user=> user.username.ToLower().Contains(username.ToLower()));
            var userList = await users.ToListAsync();
            return userList;
        }

        public async Task<User?> GetUserByIdAsync(int userId) =>
            await _context.Users.FindAsync(userId);

        public async Task<User?> GetUserByEmailAsync(string email) =>
            await _context.Users
                .AsNoTracking()
                .Where(user => user.email == email)
                .FilterUserByStatus(Status.Verified)
                .SingleOrDefaultAsync();
        

        public async Task<bool> UserVerifiedAsync(string verificationToken){
            int affectdRow = await _context.Users
                .Where(user => user.verificationToken != null && user.verificationToken.Equals(verificationToken))
                .Where(user => user.verificationTokenExpiry >= DateTime.UtcNow)
                .FilterUserByStatus(Status.Unverified)
                .ExecuteUpdateAsync(user => user
                    .SetProperty(user => user.status, Status.Verified)
                    .SetProperty(user => user.verificationToken, (String?)null)
                    .SetProperty(user => user.verificationTokenExpiry, (DateTime?)null)
                    .SetProperty(user => user.modifiedDate, DateTime.UtcNow));
            return affectdRow == 1; 
        }

        public async Task<bool> UserForgetPasswordAsync(string email, string? otp){
            int affectdRow = await _context.Users
                .Where(user => user.email.Equals(email))
                .FilterUserByStatus(Status.Verified)
                .ExecuteUpdateAsync(user => user
                    .SetProperty(user => user.otp, otp)
                    .SetProperty(user => user.otpExpiry, DateTime.UtcNow.AddMinutes(3)));
            return affectdRow == 1;
        }
        public async Task<bool>  UserResetPasswordAsync(string otp, string hashPassword){
            int affectdRow = await _context.Users
                .Where(user => user.otp != null && user.otp.Equals(otp))
                .FilterUserByStatus(Status.Verified)
                .ExecuteUpdateAsync(user => user
                    .SetProperty(user => user.password, hashPassword)
                    .SetProperty(user => user.otp, (String?)null)
                    .SetProperty(user => user.otpExpiry, (DateTime?)null)
                    .SetProperty(user => user.modifiedDate, DateTime.UtcNow));
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
            int affectdRow = await _context.Users
            .Where(user => user.userId == userId)
            .ExecuteUpdateAsync(user => user
                .SetProperty(user => user.status, Status.Banned)
                .SetProperty(user => user.modifiedDate, DateTime.UtcNow));
            return affectdRow == 1;
        }

        public async Task<bool> SoftDeleteUserAsync(int userId)
        {
            int affectdRow = await _context.Users
                .Where(user => user.userId == userId)
                .ExecuteUpdateAsync(user => user
                    .SetProperty(user => user.status, Status.Deleted)
                    .SetProperty(user => user.deletedDate, DateTime.UtcNow));
            return affectdRow == 1;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            int affectdRow = await _context.Users
                .Where(user => user.userId == userId)
                .ExecuteDeleteAsync();
            return affectdRow == 1;
        }
    }
}
