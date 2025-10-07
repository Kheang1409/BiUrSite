using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Backend.Infrastructure.Extensions;

namespace Backend.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Register all infrastructure related services (configuration adapters, persistence, messaging, repositories, etc.).
    /// This method composes smaller, single-responsibility extension methods kept in the Infrastructure.Extensions namespace.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddConfigurationServices(configuration)
            .AddPersistenceServices(configuration)
            .AddMessagingServices(configuration)
            .AddMongoDb(configuration)
            .AddRepositoryServices();

        return services;
    }

    // Backwards-compatible alias for callers that referenced the old AddPersistence extension.
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        => AddInfrastructure(services, configuration);
}