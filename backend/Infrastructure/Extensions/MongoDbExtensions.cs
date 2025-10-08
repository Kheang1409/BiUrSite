using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Backend.Infrastructure.Extensions;

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
                                    ?? configuration["MongoDB:ConnectionString"]
                                    ?? throw new ArgumentException("MongoDB ConnectionString is not configured.");
        var databaseName = Environment.GetEnvironmentVariable("MONGODB_NAME")
                            ?? configuration["MongoDB:Name"]
                            ?? throw new ArgumentException("MongoDB Name is not configured.");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        return services;
    }
}
