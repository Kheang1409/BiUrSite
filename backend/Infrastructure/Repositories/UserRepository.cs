using Backend.Domain.Enums;
using Backend.Domain.Users;
using Backend.Infrastructure.Persistence;
using MongoDB.Driver;

namespace Backend.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;
    private const int LIMIT = 10;

    public UserRepository(MongoDbContext context)
    {
        _users = context.Users;
    }

    public async Task<IEnumerable<User>> GetUsers(int pageNumber)
    {
        var filterBuilder = Builders<User>.Filter;

        var filters = new List<FilterDefinition<User>>();

        filters.Add(filterBuilder.Where(u => u.Status == Status.Active));

        var finalFilter = filters.Count > 0
            ? filterBuilder.And(filters)
            : FilterDefinition<User>.Empty;

        if (pageNumber <= 0)
            return Enumerable.Empty<User>();

        var skip = (pageNumber - 1) * LIMIT;

        return await _users.Find(finalFilter)
            .Skip(skip)
            .Limit(LIMIT)
            .ToListAsync();
    }

    public async Task<User?> GetUserById(UserId id)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        return await _users.Find(filter).FirstOrDefaultAsync();
    }
    public async Task<User?> GetUserByEmail(string email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailWithOtp(string email, string otp)
    {
        var now = DateTime.UtcNow;
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Email, email),
            Builders<User>.Filter.Eq(u => u.Otp!.Value, otp),
            Builders<User>.Filter.Gte(u => u.Otp!.ExpireAt, now)
        );

        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByToken(string token)
    {
        var now = DateTime.UtcNow;
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Token!.Value, token),
            Builders<User>.Filter.Gte(u => u.Token!.ExpireAt, now)
        );

        return await _users.Find(filter).FirstOrDefaultAsync();
    }
    public async Task Create(User user)
    {
        await _users.InsertOneAsync(user);
    }
    public async Task Update(User user)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
        await _users.ReplaceOneAsync(filter, user);
    }
}