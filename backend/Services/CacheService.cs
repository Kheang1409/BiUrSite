using StackExchange.Redis;
using System.Text.Json;

namespace Backend.Services
{
    public class CacheService: ICacheService
    {
        private readonly IDatabase _cacheDb;

        public CacheService(IConnectionMultiplexer redis)
        {
            _cacheDb = redis.GetDatabase();
        }

        public async Task SetDataAsync<T>(string key, T value, TimeSpan expiry)
        {
            var jsonData = JsonSerializer.Serialize(value);
            await _cacheDb.StringSetAsync(key, jsonData, expiry);
        }

        public async Task<T?> GetDataAsync<T>(string key)
        {
            var value = await _cacheDb.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value.ToString()) : default;
        }

        public async Task RemoveDataAsync(string key)
        {
            await _cacheDb.KeyDeleteAsync(key);
        }
    }
}