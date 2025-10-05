using Backend.Application.Services;
using Backend.Domain.Users;
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

        // Application services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IEmailService, EmailService>();

        return services;
    }
}
