using StackExchange.Redis;

namespace Backend.Services
{
    public class RedisService : IRedisService
    {
        public void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("RedisConnection")
                                    ?? configuration.GetConnectionString("RedisConnection");

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configOptions = ConfigurationOptions.Parse(connectionString);
                return ConnectionMultiplexer.Connect(configOptions);
            });
        }
    }
}
