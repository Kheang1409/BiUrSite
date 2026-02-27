using Backend.Application.Data;
using Backend.Infrastructure.Outbox;
using Backend.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Backend.Infrastructure.Extensions;

internal static class PersistenceExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<MongoDbContext>(sp => new MongoDbContext(
            sp.GetRequiredService<IConfiguration>(),
            sp.GetService<ILogger<MongoDbContext>>()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MongoDbContext>());
        
        services.AddScoped<IMongoDatabase>(sp => sp.GetRequiredService<MongoDbContext>().Database);
        
        services.AddScoped<MongoDbIndexInitializer>();
        
        services.AddHostedService<OutboxProcessor>();

        services.AddHttpContextAccessor();

        return services;
    }
}
