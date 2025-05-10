using Backend.Domain.Common.Enums;
using Backend.Domain.Users.Entities;
using Backend.Domain.Users.Interfaces;
using Backend.Infrastructure.Extensions;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Repositories;
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly int _limitItem;

    public UserRepository(AppDbContext context, IConfiguration ? configuration){
        _context = context;
        if(configuration != null)
            _limitItem = int.Parse(configuration["LimitSettings:ItemLimit"] ?? "5");
        else{
            _limitItem = 5;
        }
    }

    public async Task<List<User>> GetUsersAsync(int pageNumber, string? username){

        IQueryable<User> users = _context.Users.AsNoTracking()
            .Skip(_limitItem*pageNumber)
            .Take(_limitItem);
        if(username != null)
            users  = users.Where(user=> user.Username.ToLower().Contains(username.ToLower()));
        var userList = await users.ToListAsync();
        return userList;
    }

    public async Task<User?> GetUserByIdAsync(int userId) =>
        await _context.Users.FindAsync(userId);

    public async Task<User?> GetUserByEmailAsync(string email) =>
        await _context.Users
            .AsNoTracking()
            .Where(user => user.Email == email)
            .FilterUserByStatus(Status.Active)
            .SingleOrDefaultAsync();
    public async Task<User?> GetUserByOtpAsync(string otp) =>
        await _context.Users
            .AsNoTracking()
            .Where(user => user.Otp == otp)
            .FilterUserByStatus(Status.Active)
            .SingleOrDefaultAsync();

    public async Task<bool> VerifyUserAsync(string verificationToken){
        int affectdRow = await _context.Users
            .Where(user => user.VerificationToken != null && user.VerificationToken.Equals(verificationToken))
            .Where(user => user.VerificationTokenExpiry >= DateTime.UtcNow)
            .FilterUserByStatus(Status.Unverified)
            .ExecuteUpdateAsync(user => user
                .SetProperty(user => user.Status, Status.Active)
                .SetProperty(user => user.VerificationToken, (String?)null)
                .SetProperty(user => user.VerificationTokenExpiry, (DateTime?)null)
                .SetProperty(user => user.ModifiedDate, DateTime.UtcNow));
        return affectdRow == 1; 
    }
    public async Task<bool> RequestPasswordResetAsync(string email, string? otp){
        int affectdRow = await _context.Users
            .Where(user => user.Email.Equals(email))
            .FilterUserByStatus(Status.Active)
            .ExecuteUpdateAsync(user => user
                .SetProperty(user => user.Otp, otp)
                .SetProperty(user => user.OtpExpiry, DateTime.UtcNow.AddMinutes(3)));
        return affectdRow == 1;
    }
    public async Task<bool>  ResetUserPasswordAsync(string otp, string hashPassword){
        int affectdRow = await _context.Users
            .Where(user => user.Otp != null && user.Otp.Equals(otp))
            .FilterUserByStatus(Status.Active)
            .ExecuteUpdateAsync(user => user
                .SetProperty(user => user.Password, hashPassword)
                .SetProperty(user => user.Otp, (String?)null)
                .SetProperty(user => user.OtpExpiry, (DateTime?)null)
                .SetProperty(user => user.ModifiedDate, DateTime.UtcNow));
        return affectdRow == 1;
    }
    public async Task CreateUserAsync(User user) {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user){
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> BanUserAsync(int userId){
        int affectdRow = await _context.Users
        .Where(user => user.Id == userId)
        .ExecuteUpdateAsync(user => user
            .SetProperty(user => user.Status, Status.Banned)
            .SetProperty(user => user.ModifiedDate, DateTime.UtcNow));
        return affectdRow == 1;
    }

    public async Task<bool> SoftDeleteUserAsync(int userId)
    {
        int affectdRow = await _context.Users
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(user => user
                .SetProperty(user => user.Status, Status.Deleted)
                .SetProperty(user => user.DeletedDate, DateTime.UtcNow));
        return affectdRow == 1;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        int affectdRow = await _context.Users
            .Where(user => user.Id == userId)
            .ExecuteDeleteAsync();
        return affectdRow == 1;
    }
}