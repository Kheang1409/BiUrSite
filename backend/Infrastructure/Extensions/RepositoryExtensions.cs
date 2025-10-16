using Backend.Application.Services;
using Backend.Domain.Users;
using Backend.Domain.Posts;
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
        // Domain factories and repositories
        services.AddScoped<IUserFactory, UserFactory>();
        services.AddScoped<IUserFactory, OAuthUserFactory>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostFactory, PostFactory>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Application services
        services.AddScoped<ITokenService, JwtTokenService>();
        // RateLimiter: pick at resolve time based on presence of IConnectionMultiplexer in DI
        services.AddScoped<IRateLimiter>(sp =>
        {
            var muxer = sp.GetService<StackExchange.Redis.IConnectionMultiplexer>();
            return muxer != null ? new RedisRateLimiter(muxer) : new NoopRateLimiter();
        });
        services.AddSingleton<IEmailService, EmailService>();
        // Notifications
        services.AddSingleton<IFeedNotifier, FeedNotifier>();
        services.AddSingleton<INotificationNotifier, NotificationNotifier>();

        return services;
    }
}
