using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Backend.Infrastructure.Extensions;

namespace Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddConfigurationServices(configuration)
            .AddPersistenceServices(configuration)
            .AddMessagingServices(configuration)
            .AddRepositoryServices();

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        => AddInfrastructure(services, configuration);
}