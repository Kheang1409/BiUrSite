using Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        try
        {
            var indexInitializer = scope.ServiceProvider.GetRequiredService<MongoDbIndexInitializer>();
            await indexInitializer.EnsureIndexesAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetService<ILogger<MongoDbIndexInitializer>>();
            logger?.LogWarning(ex, "Failed to initialize database indexes. Application will continue but queries may be slower.");
        }
    }
}
