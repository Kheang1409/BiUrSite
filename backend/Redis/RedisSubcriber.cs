using Backend.Services;
using StackExchange.Redis;

namespace Backend.Redis
{
    public class RedisSubscriber : BackgroundService
    {
        private readonly ICacheService _cache;
        private readonly IConnectionMultiplexer _redis;

        public RedisSubscriber(ICacheService cache, IConnectionMultiplexer redis)
        {
            _cache = cache;
            _redis = redis;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var sub = _redis.GetSubscriber();
            await sub.SubscribeAsync(RedisChannel.Literal("cache-invalidation"), async (channel, message) =>
            {
                if (message.IsNullOrEmpty)
                {
                    Console.WriteLine("Received invalid or empty message, skipping cache invalidation.");
                    return;
                }
                string key = message.ToString();
                await _cache.RemoveDataAsync(key);
                Console.WriteLine($"Cache invalidated for key: {key}");
            });
        }
    }
}
