using Backend.Application.Data;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure.Extensions;

internal static class PersistenceExtensions
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var server = Environment.GetEnvironmentVariable("DB_SERVER")
                                ?? configuration["DB:Server"]
                                ?? throw new InvalidOperationException("DB Server is not configured.");
                                
        var port = Environment.GetEnvironmentVariable("DB_PORT")
                                ?? configuration["DB:Port"]
                                ?? throw new InvalidOperationException("DB Port is not configured.");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME")
                                ?? configuration["DB:Name"]
                                ?? throw new InvalidOperationException("DB Name is not configured.");

        var userId = Environment.GetEnvironmentVariable("DB_USER_ID")
                                ?? configuration["DB:UserId"]
                                ?? throw new InvalidOperationException("DB UserId is not configured.");
        
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD")
                                ?? configuration["DB:Password"]
                                ?? throw new InvalidOperationException("DB Password is not configured.");

        var connectionString = $"Server={server},{port};Database={dbName};User Id={userId};Password={password};TrustServerCertificate=True;";

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AppDbContext).Assembly));

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddHttpContextAccessor();

        return services;
    }
}
