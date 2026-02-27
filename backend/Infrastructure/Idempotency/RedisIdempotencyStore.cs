using System.Text.Json;
using Backend.Application.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Backend.Infrastructure.Idempotency;

public sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisIdempotencyStore> _logger;
    private const string KeyPrefix = "idempotency:";
    private const string InProgressSuffix = ":inprogress";

    public RedisIdempotencyStore(IConnectionMultiplexer redis, ILogger<RedisIdempotencyStore> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<(bool Exists, object? Response)> TryGetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync($"{KeyPrefix}{key}");
            
            if (value.IsNullOrEmpty)
                return (false, null);

            var stringValue = (string)value!;
            var wrapper = JsonSerializer.Deserialize<ResponseWrapper>(stringValue);
            return (true, wrapper?.Response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get idempotency key {Key} from Redis", key);
            return (false, null);
        }
    }

    public async Task SetAsync(string key, object? response, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var wrapper = new ResponseWrapper(response, DateTime.UtcNow);
            var json = JsonSerializer.Serialize(wrapper);
            
            await db.StringSetAsync($"{KeyPrefix}{key}", json, expiry);
            await db.KeyDeleteAsync($"{KeyPrefix}{key}{InProgressSuffix}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set idempotency key {Key} in Redis", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync($"{KeyPrefix}{key}");
            await db.KeyDeleteAsync($"{KeyPrefix}{key}{InProgressSuffix}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove idempotency key {Key} from Redis", key);
        }
    }

    public async Task<bool> TryMarkInProgressAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var inProgressKey = $"{KeyPrefix}{key}{InProgressSuffix}";
            
            return await db.StringSetAsync(inProgressKey, "1", expiry, When.NotExists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark idempotency key {Key} as in-progress in Redis", key);
            return true;
        }
    }

    private sealed record ResponseWrapper(object? Response, DateTime CreatedAt);
}
