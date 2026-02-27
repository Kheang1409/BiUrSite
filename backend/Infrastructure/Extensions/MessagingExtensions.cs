using Backend.Application.Comments.Create;
using Backend.Application.Posts.Create;
using Backend.Application.Posts.Delete;
using Backend.Application.Users.Create;
using Backend.Application.Users.ForgotPassword;
using Backend.Application.Users.Update;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace Backend.Infrastructure.Extensions;

internal static class MessagingExtensions
{
    public static IServiceCollection AddMessagingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
                                    ?? configuration["MongoDB:ConnectionString"]
                                    ?? throw new ArgumentException("MongoDB ConnectionString is not configured.");

        var databaseName = Environment.GetEnvironmentVariable("MONGODB_NAME")
                           ?? configuration["MongoDB:Name"]
                           ?? throw new ArgumentException("MongoDB Name is not configured.");

        var queueName = Environment.GetEnvironmentVariable("REBUS_QUEUE_NAME")
                        ?? configuration["Rebus:QueueName"]
                        ?? throw new ArgumentException("Rebus QueueName is not configured.");
        try
        {
            var builder = new MongoUrlBuilder(mongoConnectionString);
            if (string.IsNullOrWhiteSpace(builder.DatabaseName))
            {
                builder.DatabaseName = databaseName;
                mongoConnectionString = builder.ToString();
            }
        }
        catch
        {
            mongoConnectionString = mongoConnectionString.TrimEnd('/') + "/" + databaseName;
        }

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
        services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

        services.AddRebus(configure =>
        {
            var transportCollectionName = queueName;
            var mongoOptions = new MongoDbTransportOptions(mongoConnectionString, transportCollectionName);

            configure
                .Transport(t => t.UseMongoDb(mongoOptions, queueName))
                .Routing(r => r.TypeBased()
                    .Map<UserCreatedEvent>(queueName)
                    .Map<UserForgotPasswordEvent>(queueName)
                    .Map<UpdatedProfileEvent>(queueName)
                    .Map<PostCreatedEvent>(queueName)
                    .Map<PostDeletedEvent>(queueName)
                    .Map<CommentCreatedEvent>(queueName)
                )
                .Options(o =>
                {
                    o.SetNumberOfWorkers(1);
                    o.SetMaxParallelism(1);
                });

            return configure;
        });

        return services;
    }
}
