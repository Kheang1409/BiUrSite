using Backend.Application.Services;
using Backend.Domain.Comments.Interfaces;
using Backend.Domain.Notifications.Interfaces;
using Backend.Domain.Posts.Interfaces;
using Backend.Domain.Services;
using Backend.Domain.Users.Factories;
using Backend.Domain.Users.Interfaces;
using Backend.Infrastructure.Configurations;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bacnkend.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
        
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECT_STRING")
                                ?? configuration.GetConnectionString("DB_CONNECT_STRING")
                                ?? throw new InvalidOperationException("DB_CONNECT_STRING is not configured.");
        Console.WriteLine($"DB_CONNECT_STRING: {connectionString}");
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
            })
        );
            
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IUserFactory, UserFactory>();
        services.AddScoped<OAuthUserFactory>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }
}
