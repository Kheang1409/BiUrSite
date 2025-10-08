using Backend.Application.Data;
using Backend.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure.Extensions;

internal static class PersistenceExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<MongoDbContext>(sp => new MongoDbContext(
            sp.GetRequiredService<IConfiguration>(),
            sp.GetRequiredService<IPublisher>()));

        // Expose the MongoDbContext as the application abstractions.
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<MongoDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MongoDbContext>());

        services.AddHttpContextAccessor();

        return services;
    }
}
