using Backend.Domain.Users;
using Backend.Infrastructure.Persistence;
using MongoDB.Driver;

namespace Backend.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;
    private const int LIMIT_ITEM = 10;

    public UserRepository(MongoDbContext context)
    {
        _users = context.Users;
    }

    public async Task<User?> GetUserById(UserId id)
    {
        return await _users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync();
    }
    public async Task<User?> GetUserByEmail(string email)
    {
        return await _users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailWithOtp(string email, string otp)
    {
        var now = DateTime.UtcNow;
        return await _users
            .Find(u => u.Email == email &&
                       u.Otp != null &&
                       u.Otp.Value == otp &&
                       u.Otp.ExpireAt >= now)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByToken(string token)
    {
        var now = DateTime.UtcNow;
        return await _users
            .Find(u => u.Token != null &&
                       u.Token.Value == token &&
                       u.Token.ExpireAt >= now)
            .FirstOrDefaultAsync();
    }
    public async Task Create(User user)
    {
        await _users.InsertOneAsync(user); ;
    }
    public async Task Update(User user)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
        await _users.ReplaceOneAsync(filter, user);
    }
}