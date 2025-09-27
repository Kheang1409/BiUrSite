using Backend.Application.Configuration;
using Backend.Application.Data;
using Backend.Application.Services;
using Backend.Application.Users.CreateUser;
using Backend.Application.Users.ForgotPassword;
using Backend.Domain.Users;
using Backend.Infrastructure.Configuration;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
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

        var appBaseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL")
                            ?? configuration["App:BaseUrl"]
                            ?? throw new InvalidOperationException("App BaseUrl is not configured.");
                            

        var connectionString = $"Server={server},{port};Database={dbName};User Id={userId};Password={password};TrustServerCertificate=True;";

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AppDbContext).Assembly));


        services.AddRebus(configure =>
        {
            configure.Transport(t =>
                t.UseSqlServer(connectionString, "queue-name", ensureTablesAreCreated: true)
            );

            configure.Routing(r => r.TypeBased()
                .Map<UserCreatedEvent>("queue-name")
                .Map<UserForgotPasswordEvent>("queue-name")
            );

            configure.Options(o =>
            {
                o.SetNumberOfWorkers(1);
                o.SetMaxParallelism(1);
            });

            return configure;
        });

        services.Configure<AppOptions>(options =>
        {
            options.BaseUrl = appBaseUrl;
        });
        
        services.AddSingleton<IAppOptions, AppOptionsAdapter>();

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseSqlServer(connectionString));

        services.AddScoped<IAppDbContext>(
            sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IUnitOfWork>(
            sp => sp.GetRequiredService<AppDbContext>());

        services.AddHttpContextAccessor();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUserFactory, UserFactory>();
        services.AddScoped<IUserFactory, OAuthUserFactory>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IEmailService, EmailService>();

        return services;
    }
}