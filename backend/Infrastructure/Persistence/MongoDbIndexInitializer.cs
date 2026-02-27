using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.Infrastructure.Outbox;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Backend.Infrastructure.Persistence;

public sealed class MongoDbIndexInitializer
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Post> _posts;
    private readonly IMongoCollection<OutboxMessage> _outbox;
    private readonly ILogger<MongoDbIndexInitializer> _logger;

    public MongoDbIndexInitializer(MongoDbContext context, ILogger<MongoDbIndexInitializer> logger)
    {
        _users = context.Users;
        _posts = context.Posts;
        _outbox = context.Database.GetCollection<OutboxMessage>("outbox_messages");
        _logger = logger;
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ensuring MongoDB indexes...");

        try
        {
            await CreateUserIndexesAsync(cancellationToken);
            await CreatePostIndexesAsync(cancellationToken);
            await CreateOutboxIndexesAsync(cancellationToken);
            
            _logger.LogInformation("MongoDB indexes ensured successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create MongoDB indexes. Queries may be slow.");
        }
    }

    private async Task CreateUserIndexesAsync(CancellationToken cancellationToken)
    {
        var emailIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true, Name = "idx_users_email_unique" });

        var tokenIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys
                .Ascending(u => u.Token!.Value)
                .Ascending(u => u.Token!.ExpireAt),
            new CreateIndexOptions { Name = "idx_users_token_expiry", Sparse = true });

        var otpIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys
                .Ascending(u => u.Otp!.Value)
                .Ascending(u => u.Otp!.ExpireAt),
            new CreateIndexOptions { Name = "idx_users_otp_expiry", Sparse = true });

        var statusIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Status),
            new CreateIndexOptions { Name = "idx_users_status" });

        await _users.Indexes.CreateManyAsync(
            [emailIndex, tokenIndex, otpIndex, statusIndex],
            cancellationToken);
    }

    private async Task CreatePostIndexesAsync(CancellationToken cancellationToken)
    {
        var listingIndex = new CreateIndexModel<Post>(
            Builders<Post>.IndexKeys
                .Ascending(p => p.Status)
                .Descending(p => p.CreatedDate),
            new CreateIndexOptions { Name = "idx_posts_status_created" });

        var userPostsIndex = new CreateIndexModel<Post>(
            Builders<Post>.IndexKeys
                .Ascending(p => p.UserId)
                .Ascending(p => p.Status)
                .Descending(p => p.CreatedDate),
            new CreateIndexOptions { Name = "idx_posts_user_status_created" });

        var textIndex = new CreateIndexModel<Post>(
            Builders<Post>.IndexKeys.Text(p => p.Text),
            new CreateIndexOptions { Name = "idx_posts_text_search" });

        await _posts.Indexes.CreateManyAsync(
            [listingIndex, userPostsIndex, textIndex],
            cancellationToken);
    }

    private async Task CreateOutboxIndexesAsync(CancellationToken cancellationToken)
    {
        var unprocessedIndex = new CreateIndexModel<OutboxMessage>(
            Builders<OutboxMessage>.IndexKeys
                .Ascending(m => m.ProcessedOnUtc)
                .Ascending(m => m.RetryCount)
                .Ascending(m => m.OccurredOnUtc),
            new CreateIndexOptions { Name = "idx_outbox_unprocessed" });

        await _outbox.Indexes.CreateManyAsync(
            [unprocessedIndex],
            cancellationToken);
    }
}
