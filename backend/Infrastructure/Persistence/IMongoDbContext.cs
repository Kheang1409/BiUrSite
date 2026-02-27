using Backend.Domain.Posts;
using Backend.Domain.Users;
using MongoDB.Driver;

namespace Backend.Infrastructure.Persistence;

internal interface IMongoDbContext
{
    IMongoCollection<User> Users { get; }
    IMongoCollection<Post> Posts { get; }
    IMongoDatabase Database { get; }
    IMongoClient Client { get; }
    IClientSessionHandle StartSession();
    Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default);
}
