using StackExchange.Redis;

namespace Backend.Services
{
    public class RedisService : IRedisService
    {
        public void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("RedisConnection")
                                    ?? configuration.GetConnectionString("RedisConnection");

            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Redis connection string is not provided.");
            }
            
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configOptions = ConfigurationOptions.Parse(connectionString);
                return ConnectionMultiplexer.Connect(configOptions);
            });
        }
    }
}
