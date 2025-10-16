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
        var requestLimit = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_REQUESTS"), out var rl)
            ? rl
            : int.Parse(configuration["LimitSettings:RequestLimit"] ?? "100");
        var windowSeconds = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_WINDOW_SECONDS"), out var ws)
            ? ws
            : 60;
        services.Configure<Backend.Application.Configuration.RateLimitOptions>(opt =>
        {
            opt.RequestLimit = requestLimit;
            opt.WindowSeconds = windowSeconds;
        });

        var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
                        ?? configuration["Redis:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConn));
        }
        services.AddHttpClient();
        services.AddScoped<IImageStorageService, GitHubImageStorageService>();
        services.AddSingleton<IAppOptions, Configuration.AppOptionsAdapter>();
        return services;
    }
}
