using Backend.Application.Services;
using StackExchange.Redis;

namespace Backend.Infrastructure.RateLimiting;

public class RedisRateLimiter : IRateLimiter, IDisposable
{
    private readonly IConnectionMultiplexer _muxer;

    public RedisRateLimiter(IConnectionMultiplexer muxer)
    {
        _muxer = muxer;
    }

    public async Task<bool> ShouldAllowAsync(string key, int limit, TimeSpan window, CancellationToken ct = default)
    {
        var db = _muxer.GetDatabase();
        var nowBucket = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / (long)window.TotalSeconds;
        var redisKey = new RedisKey($"ratelimit:{key}:{nowBucket}");

        // INCR and set TTL atomically via Lua script
        const string script = @"
local current = redis.call('INCR', KEYS[1])
if tonumber(current) == 1 then
  redis.call('EXPIRE', KEYS[1], ARGV[1])
end
return current
";
        var result = (long)await db.ScriptEvaluateAsync(script, new RedisKey[] { redisKey }, new RedisValue[] { (int)window.TotalSeconds }).ConfigureAwait(false);
        return result <= limit;
    }

    public void Dispose()
    {
        _muxer.Dispose();
    }
}
