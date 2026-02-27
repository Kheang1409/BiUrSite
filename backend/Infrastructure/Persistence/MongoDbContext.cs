using System.Text.Json;
using Backend.Application.Data;
using Backend.Domain.Posts;
using Backend.Domain.Primitive;
using Backend.Domain.Users;
using Backend.Infrastructure.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Backend.Infrastructure.Persistence;

public sealed class MongoDbContext : IMongoDbContext, IUnitOfWork
{
    private readonly ILogger<MongoDbContext>? _logger;
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private const string USERS_DOCUMENT = "users";
    private const string POSTS_DOCUMENT = "posts";
    private const string OUTBOX_DOCUMENT = "outbox_messages";
    
    public IMongoCollection<User> Users => _database.GetCollection<User>(USERS_DOCUMENT);
    public IMongoCollection<Post> Posts => _database.GetCollection<Post>(POSTS_DOCUMENT);
    public IMongoDatabase Database => _database;
    public IMongoClient Client => _client;

    public MongoDbContext(IConfiguration configuration, ILogger<MongoDbContext>? logger = null)
    {
        _logger = logger;

        var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
                                    ?? configuration["MongoDB:ConnectionString"]
                                    ?? throw new ArgumentException("MongoDB ConnectionString is not configured.");
        var databaseName = Environment.GetEnvironmentVariable("MONGODB_NAME")
                            ?? configuration["MongoDB:Name"]
                            ?? throw new ArgumentException("MongoDB Name is not configured.");

        _client = new MongoClient(mongoConnectionString);
        _database = _client.GetDatabase(databaseName);
    }

    public IClientSessionHandle StartSession() => _client.StartSession();
    
    public async Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default)
        => await _client.StartSessionAsync(cancellationToken: cancellationToken);

    public async Task SaveChangesAsync(Entity entity, CancellationToken cancellationToken = default)
    {
        var domainEvents = entity.GetDomainEvents().ToList();
        entity.ClearDomainEvents();
        
        if (domainEvents.Count == 0)
        {
            return;
        }

        var outboxCollection = _database.GetCollection<OutboxMessage>(OUTBOX_DOCUMENT);
        var outboxMessages = domainEvents.Select(e => new OutboxMessage
        {
            Id = e.Id,
            EventType = e.GetType().FullName ?? e.GetType().Name,
            Payload = JsonSerializer.Serialize(e, e.GetType()),
            OccurredOnUtc = e.OccurredOn,
            ProcessedOnUtc = null,
            Error = null,
            RetryCount = 0
        }).ToList();

        try
        {
            await outboxCollection.InsertManyAsync(outboxMessages, cancellationToken: cancellationToken);
            _logger?.LogDebug(
                "Saved {EventCount} domain events to outbox for entity",
                outboxMessages.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Failed to save {EventCount} domain events to outbox",
                outboxMessages.Count);
            throw;
        }
    }
}
