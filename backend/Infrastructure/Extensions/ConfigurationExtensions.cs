using Backend.Application.Configuration;
using Backend.Application.Storage;
using Backend.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Backend.Infrastructure.Extensions;

internal static class ConfigurationExtensions
{
    public static IServiceCollection AddConfigurationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var appBaseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL")
                        ?? configuration["App:BaseUrl"]
                        ?? throw new InvalidOperationException("App BaseUrl is not configured.");

        services.Configure<Configuration.AppOptions>(options =>
        {
            options.BaseUrl = appBaseUrl;
        });
        
        // Rate limit options (env overrides appsettings)
        var requestLimitStr = Environment.GetEnvironmentVariable("RATE_LIMIT_REQUESTS")
                            ?? configuration["LimitSettings:RequestLimit"]
                            ?? throw new InvalidOperationException("Rate Limit RequestLimit is not configured.");
        
        var windowSecondsStr = Environment.GetEnvironmentVariable("RATE_LIMIT_WINDOW_SECONDS")
                             ?? configuration["LimitSettings:WindowSeconds"]
                             ?? throw new InvalidOperationException("Rate Limit WindowSeconds is not configured.");
        
        var requestLimit = int.Parse(requestLimitStr);
        var windowSeconds = int.Parse(windowSecondsStr);
        
        services.Configure<RateLimitOptions>(opt =>
        {
            opt.RequestLimit = requestLimit;
            opt.WindowSeconds = windowSeconds;
        });

        var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
                        ?? configuration["Redis:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var options = ConfigurationOptions.Parse(redisConn);
                options.AbortOnConnectFail = false; // Allow app to start even if Redis is unavailable
                options.ConnectTimeout = 5000; // 5 seconds timeout
                options.ConnectRetry = 3;
                
                var multiplexer = ConnectionMultiplexer.Connect(options);
                
                // Log connection status
                if (multiplexer.IsConnected)
                {
                    Console.WriteLine($"Successfully connected to Redis at {redisConn}");
                }
                else
                {
                    Console.WriteLine($"Warning: Redis connection established but not connected to {redisConn}");
                    Console.WriteLine("Rate limiting will fall back to NoopRateLimiter");
                }
                
                return multiplexer;
            });
        }
        services.AddHttpClient();
        services.AddScoped<IImageStorageService, GitHubImageStorageService>();
        services.AddSingleton<IAppOptions, Configuration.AppOptionsAdapter>();
        return services;
    }
}
