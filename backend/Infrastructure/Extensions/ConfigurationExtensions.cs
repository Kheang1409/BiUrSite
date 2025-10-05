using Backend.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddSingleton<IAppOptions, Configuration.AppOptionsAdapter>();

        return services;
    }
}
