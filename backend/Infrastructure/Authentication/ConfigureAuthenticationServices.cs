using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Backend.Infrastructure.Authentication.Providers;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Infrastructure.Authentication;

public static class AuthenticationServiceConfiguration
{
    public static IServiceCollection ConfigureAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        context.Token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                                        ? authHeader.Substring("Bearer ".Length).Trim()
                                        : authHeader;
                    }

                    if (string.IsNullOrEmpty(context.Token) && context.Request.Query.TryGetValue("access_token", out var accessToken))
                    {
                        var path = context.HttpContext.Request.Path;
                        if (path.StartsWithSegments("/notificationHub", StringComparison.OrdinalIgnoreCase) ||
                            path.StartsWithSegments("/feedHub", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = accessToken;
                        }
                    }

                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = ctx =>
                {
                    if (string.Equals(Environment.GetEnvironmentVariable("ENABLE_REQUEST_TRACING"), "1"))
                    {
                        var logger = ctx.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("JwtAuthentication");
                        logger?.LogWarning("JWT Failed: {Message}", ctx.Exception.Message);
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = ctx =>
                {
                    if (string.Equals(Environment.GetEnvironmentVariable("ENABLE_REQUEST_TRACING"), "1"))
                    {
                        var logger = ctx.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("JwtAuthentication");
                        logger?.LogDebug("JWT Validated for {Name}", ctx.Principal?.Identity?.Name);
                    }
                    return Task.CompletedTask;
                }
            };

            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                            ?? configuration["JWT:SecretKey"]
                            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                        ?? configuration["JWT:Issuer"]
                        ?? throw new InvalidOperationException("JWT Issuer is not configured.");
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                        ?? configuration["JWT:Audience"]
                        ?? throw new InvalidOperationException("JWT Audience is not configured.");

            var key = Encoding.UTF8.GetBytes(secretKey);

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = ClaimTypes.Role
            };
        })
        .AddCookie();

        var externalProviders = new List<IAuthenticationProviderConfigurator>
        {
            new GoogleAuthenticationConfigurator(),
            new FacebookAuthenticationConfigurator(),
        };

        foreach (var provider in externalProviders)
        {
            provider.Configure(authBuilder, configuration);
        }

        services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();
        
        return services;
    }
}
