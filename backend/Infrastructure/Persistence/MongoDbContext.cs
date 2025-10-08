using Backend.Application.Data;
using Backend.Domain.Posts;
using Backend.Domain.Primitive;
using Backend.Domain.Users;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Backend.Infrastructure.Persistence;

public class MongoDbContext : IAppDbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;
    private readonly IMongoDatabase _database;
    private const string USERS_DOCUMENT = "users";
    private const string POSTS_DOCUMENT = "posts";
    public IMongoCollection<User> Users => _database.GetCollection<User>(USERS_DOCUMENT);
    public IMongoCollection<Post> Posts => _database.GetCollection<Post>(POSTS_DOCUMENT);

    public MongoDbContext(IConfiguration configuration, IPublisher publisher)
    {
        _publisher = publisher;

        var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
                                    ?? configuration["MongoDB:ConnectionString"]
                                    ?? throw new ArgumentException("MongoDB ConnectionString is not configured.");
        var databaseName = Environment.GetEnvironmentVariable("MONGODB_NAME")
                            ?? configuration["MongoDB:Name"]
                            ?? throw new ArgumentException("MongoDB Name is not configured.");

        var mongoClient = new MongoClient(mongoConnectionString);
        _database = mongoClient.GetDatabase(databaseName);
    }

    public async Task SaveChangesAsync(Entity entity, CancellationToken cancellationToken = default)
    {
        var domainEvents = entity.GetDomainEvents().ToList();
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        entity.ClearDomainEvents();
    }
}
