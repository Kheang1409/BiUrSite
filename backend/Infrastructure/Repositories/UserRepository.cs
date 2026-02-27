using Backend.Domain.Enums;
using Backend.Domain.Users;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Backend.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<UserRepository> _logger;
    private const int LIMIT = 10;

    public UserRepository(MongoDbContext context, ILogger<UserRepository> logger)
    {
        _users = context.Users;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetUsers(int pageNumber)
    {
        if (pageNumber <= 0)
            return Enumerable.Empty<User>();

        var filterBuilder = Builders<User>.Filter;
        var filters = new List<FilterDefinition<User>>
        {
            filterBuilder.Where(u => u.Status == Status.Active)
        };
        var finalFilter = filterBuilder.And(filters);
        var skip = (pageNumber - 1) * LIMIT;

        return await RetryPolicy.ExecuteWithRetryAsync(
            () => _users.Find(finalFilter).Skip(skip).Limit(LIMIT).ToListAsync(),
            logger: _logger,
            operationName: "GetUsers");
    }

    public Task<User?> GetUserById(UserId id)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        return RetryPolicy.ExecuteWithRetryAsync(
            () => _users.Find(filter).FirstOrDefaultAsync(),
            logger: _logger,
            operationName: "GetUserById")!;
    }

    public Task<User?> GetUserByEmail(string email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        return RetryPolicy.ExecuteWithRetryAsync(
            () => _users.Find(filter).FirstOrDefaultAsync(),
            logger: _logger,
            operationName: "GetUserByEmail")!;
    }

    public Task<User?> GetUserByEmailWithOtp(string email, string otp)
    {
        var now = DateTime.UtcNow;
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Email, email),
            Builders<User>.Filter.Eq(u => u.Otp!.Value, otp),
            Builders<User>.Filter.Gte(u => u.Otp!.ExpireAt, now));

        return RetryPolicy.ExecuteWithRetryAsync(
            () => _users.Find(filter).FirstOrDefaultAsync(),
            logger: _logger,
            operationName: "GetUserByEmailWithOtp")!;
    }

    public Task<User?> GetUserByToken(string token)
    {
        var now = DateTime.UtcNow;
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Token!.Value, token),
            Builders<User>.Filter.Gte(u => u.Token!.ExpireAt, now));

        return RetryPolicy.ExecuteWithRetryAsync(
            () => _users.Find(filter).FirstOrDefaultAsync(),
            logger: _logger,
            operationName: "GetUserByToken")!;
    }

    public Task Create(User user)
    {
        return RetryPolicy.ExecuteWithRetryAsync(
            () => _users.InsertOneAsync(user),
            logger: _logger,
            operationName: "CreateUser");
    }

    public Task Update(User user)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
        return RetryPolicy.ExecuteWithRetryAsync(
            () => _users.ReplaceOneAsync(filter, user),
            logger: _logger,
            operationName: "UpdateUser");
    }
}