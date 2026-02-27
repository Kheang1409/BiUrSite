using Backend.Application.Services;
using Backend.Domain.Users;
using Backend.Domain.Posts;
using Backend.Infrastructure.Authentication;
using Backend.Infrastructure.Idempotency;
using Backend.Infrastructure.Notifications;
using Backend.Infrastructure.Repositories;
using Backend.Infrastructure.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Backend.Domain.Comments;
using Backend.Domain.Notifications;

namespace Backend.Infrastructure.Extensions;

internal static class RepositoryExtensions
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddScoped<IStandardUserFactory, UserFactory>();
        services.AddScoped<IOAuthUserFactory, OAuthUserFactory>();
        services.AddScoped<IPostFactory, PostFactory>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, Sha512PasswordHasher>();
        
        services.AddSingleton<IRateLimiter>(sp =>
        {
            var muxer = sp.GetService<StackExchange.Redis.IConnectionMultiplexer>();
            if (muxer != null && muxer.IsConnected)
            {
                return new RedisRateLimiter(muxer);
            }
            return new NoopRateLimiter();
        });
        
        services.AddSingleton<IIdempotencyStore>(sp =>
        {
            var muxer = sp.GetService<StackExchange.Redis.IConnectionMultiplexer>();
            if (muxer != null && muxer.IsConnected)
            {
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RedisIdempotencyStore>>();
                return new RedisIdempotencyStore(muxer, logger);
            }
            return new InMemoryIdempotencyStore();
        });
        
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<IFeedNotifier, FeedNotifier>();
        services.AddSingleton<INotificationNotifier, NotificationNotifier>();

        return services;
    }
}
