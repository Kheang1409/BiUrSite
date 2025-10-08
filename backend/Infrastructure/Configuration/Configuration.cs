using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure.Configuration;
public static class CorsConfiguration
{
    public const string AllowFrontendPolicy = "AllowFrontend";

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var envOrigins = Environment.GetEnvironmentVariable("ALLOWED_CORS");
            string[] origins;

            if (!string.IsNullOrWhiteSpace(envOrigins))
            {
                origins = envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
            else
            {
                origins = configuration.GetSection("AllowedCors").Get<string[]>()!;
            }

            if (origins == null || origins.Length == 0)
            {
                throw new InvalidOperationException("AllowedCors is not configured.");
            }

            services.AddCors(options =>
            {
                options.AddPolicy(AllowFrontendPolicy, policy =>
                {
                    policy
                        .WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

        return services;
    }
}