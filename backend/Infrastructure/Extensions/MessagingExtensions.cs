using Backend.Application.Posts.Create;
using Backend.Application.Posts.Delete;
using Backend.Application.Users.Create;
using Backend.Application.Users.ForgotPassword;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace Backend.Infrastructure.Extensions;

internal static class MessagingExtensions
{
    public static IServiceCollection AddMessagingServices(this IServiceCollection services, IConfiguration configuration)
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

        services.AddRebus(configure =>
        {
            // UseSqlServer overload is currently obsolete in Rebus package used by this project.
            // Keep current call for compatibility; suppress obsolete warning in this file.
            #pragma warning disable 618
            configure.Transport(t => t.UseSqlServer(connectionString, "queue-notify", ensureTablesAreCreated: true));
            #pragma warning restore 618

            configure.Routing(r => r.TypeBased()
                .Map<UserCreatedEvent>("queue-notify")
                .Map<UserForgotPasswordEvent>("queue-notify")
                .Map<PostCreatedEvent>("queue-notify")
                .Map<PostDeletedEvent>("queue-notify")
            );

            configure.Options(o =>
            {
                o.SetNumberOfWorkers(1);
                o.SetMaxParallelism(1);
            });

            return configure;
        });

        return services;
    }
}
