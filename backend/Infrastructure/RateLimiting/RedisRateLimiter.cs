using Backend.Application.Services;
using StackExchange.Redis;

namespace Backend.Infrastructure.RateLimiting;

public sealed class RedisRateLimiter : IRateLimiter
{
    private readonly IConnectionMultiplexer _muxer;

    public RedisRateLimiter(IConnectionMultiplexer muxer)
    {
        _muxer = muxer ?? throw new ArgumentNullException(nameof(muxer));
    }

    public async Task<bool> ShouldAllowAsync(string key, int limit, TimeSpan window, CancellationToken ct = default)
    {
        var db = _muxer.GetDatabase();
        var nowBucket = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / (long)window.TotalSeconds;
        var redisKey = new RedisKey($"ratelimit:{key}:{nowBucket}");

        const string script = """
            local current = redis.call('INCR', KEYS[1])
            if tonumber(current) == 1 then
              redis.call('EXPIRE', KEYS[1], ARGV[1])
            end
            return current
            """;
        var result = (long)await db.ScriptEvaluateAsync(script, [redisKey], [(int)window.TotalSeconds]).ConfigureAwait(false);
        return result <= limit;
    }
}
