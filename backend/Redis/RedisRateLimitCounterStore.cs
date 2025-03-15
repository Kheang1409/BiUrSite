using AspNetCoreRateLimit;
using StackExchange.Redis;

namespace Backend.Redis
{
    public class RedisRateLimitCounterStore : IRateLimitCounterStore
    {
        private readonly IDatabase _database;
        private readonly string _prefix = "rateLimit:";

        public RedisRateLimitCounterStore(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            var redisKey = $"{_prefix}{id}";
            return await _database.KeyExistsAsync(redisKey);
        }

        public async Task<RateLimitCounter?> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            var redisKey = $"{_prefix}{id}";
            var counter = await _database.HashGetAsync(redisKey, "counter");

            if (counter.IsNullOrEmpty)
                return null;

            var timestamp = await _database.HashGetAsync(redisKey, "timestamp");

            return new RateLimitCounter
            {
                Count = (int)counter,
                Timestamp = timestamp.HasValue ? DateTime.Parse(timestamp.ToString()) : DateTime.MinValue
            };
        }

        public async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            var redisKey = $"{_prefix}{id}";
            await _database.KeyDeleteAsync(redisKey);
        }

        public async Task SetAsync(string id, RateLimitCounter? entry, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
        {
            var redisKey = $"{_prefix}{id}";

            if (entry == null)
                return;

            var values = new HashEntry[]
            {
                new HashEntry("counter", entry?.Count),
                new HashEntry("timestamp", entry?.Timestamp.ToString()) 
            };

            await _database.HashSetAsync(redisKey, values);

            if (expirationTime.HasValue)
            {
                await _database.KeyExpireAsync(redisKey, expirationTime.Value);
            }
        }
    }
}
