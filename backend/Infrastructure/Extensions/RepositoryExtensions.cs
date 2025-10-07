using Backend.Application.Services;
using Backend.Domain.Users;
using Backend.Domain.Posts;
using Backend.Infrastructure.Notifications;
using Backend.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

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

        // Application services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IEmailService, EmailService>();
        // Notifications
        services.AddSingleton<IFeedNotifier, FeedNotifier>();

        return services;
    }
}
