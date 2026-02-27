using System.Text.Json;
using Backend.Domain.Primitive;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Backend.Infrastructure.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval;
    private const int BatchSize = 20;
    private const int MaxRetryCount = 3;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        var pollingMs = configuration.GetValue("Outbox:PollingIntervalMs", 5000);
        _pollingInterval = TimeSpan.FromMilliseconds(pollingMs);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox processor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        
        var collection = database.GetCollection<OutboxMessage>("outbox_messages");
        
        var filter = Builders<OutboxMessage>.Filter.And(
            Builders<OutboxMessage>.Filter.Eq(m => m.ProcessedOnUtc, null),
            Builders<OutboxMessage>.Filter.Lt(m => m.RetryCount, MaxRetryCount));
        
        var sort = Builders<OutboxMessage>.Sort.Ascending(m => m.OccurredOnUtc);
        
        var messages = await collection
            .Find(filter)
            .Sort(sort)
            .Limit(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var eventType = ResolveEventType(message.EventType);
                if (eventType == null)
                {
                    _logger.LogWarning(
                        "Could not resolve event type {EventType} for outbox message {MessageId}",
                        message.EventType,
                        message.Id);
                    await MarkAsFailedAsync(collection, message, "Unknown event type", cancellationToken);
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType) as DomainEvent;
                if (domainEvent == null)
                {
                    _logger.LogWarning(
                        "Could not deserialize outbox message {MessageId}",
                        message.Id);
                    await MarkAsFailedAsync(collection, message, "Deserialization failed", cancellationToken);
                    continue;
                }

                await publisher.Publish(domainEvent, cancellationToken);
                await MarkAsProcessedAsync(collection, message, cancellationToken);
                
                _logger.LogDebug(
                    "Processed outbox message {MessageId} of type {EventType}",
                    message.Id,
                    message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process outbox message {MessageId}, retry count: {RetryCount}",
                    message.Id,
                    message.RetryCount);
                await IncrementRetryAsync(collection, message, ex.Message, cancellationToken);
            }
        }
    }

    private static Type? ResolveEventType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null && typeof(DomainEvent).IsAssignableFrom(type))
            {
                return type;
            }
        }
        return null;
    }

    private static async Task MarkAsProcessedAsync(
        IMongoCollection<OutboxMessage> collection,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var update = Builders<OutboxMessage>.Update
            .Set(m => m.ProcessedOnUtc, DateTime.UtcNow);
        
        await collection.UpdateOneAsync(
            m => m.Id == message.Id,
            update,
            cancellationToken: cancellationToken);
    }

    private static async Task MarkAsFailedAsync(
        IMongoCollection<OutboxMessage> collection,
        OutboxMessage message,
        string error,
        CancellationToken cancellationToken)
    {
        var update = Builders<OutboxMessage>.Update
            .Set(m => m.ProcessedOnUtc, DateTime.UtcNow)
            .Set(m => m.Error, error);
        
        await collection.UpdateOneAsync(
            m => m.Id == message.Id,
            update,
            cancellationToken: cancellationToken);
    }

    private static async Task IncrementRetryAsync(
        IMongoCollection<OutboxMessage> collection,
        OutboxMessage message,
        string error,
        CancellationToken cancellationToken)
    {
        var update = Builders<OutboxMessage>.Update
            .Inc(m => m.RetryCount, 1)
            .Set(m => m.Error, error);
        
        await collection.UpdateOneAsync(
            m => m.Id == message.Id,
            update,
            cancellationToken: cancellationToken);
    }
}
