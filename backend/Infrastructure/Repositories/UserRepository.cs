using Backend.Domain.Users;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private const int LIMIT_ITEM = 10;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetUserById(UserId id)
    {
        return _context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id);
    }
    public Task<User?> GetUserByEmail(string email)
    {
        return _context.Users.SingleOrDefaultAsync(u => u.Email.Equals(email));
    }

    public Task<User?> GetUserByEmailWithOtp(string email, string otp)
    {
        return _context.Users.SingleOrDefaultAsync(u => u.Email.Equals(email) && u.Otp!.Value.Equals(otp) && u.Otp!.ExpireAt >= DateTime.UtcNow);
    }

    public Task<User?> GetUserByToken(string token)
    {
        return _context.Users.SingleOrDefaultAsync(u => u.Token!.Value.Equals(token) && u.Token.ExpireAt >= DateTime.UtcNow);
    }
    public void Create(User user)
    {
        _context.Users.AddAsync(user);
    }
}