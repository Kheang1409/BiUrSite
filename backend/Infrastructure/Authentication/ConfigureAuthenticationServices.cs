using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Infrastructure.Authentication.Providers;
using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Authentication;

public static class AuthenticationServiceConfiguration
{
    public static IServiceCollection ConfigureAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT + Cookie base setup
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
                    var token = context.Request.Headers["Authorization"].ToString();

                    if (!string.IsNullOrEmpty(token))
                    {
                        // Accept token exactly as sent (with "Bearer " prefix)
                        // Don't strip "Bearer ", let JwtBearer middleware handle it
                        context.Token = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                                        ? token.Substring("Bearer ".Length).Trim()
                                        : token;
                    }

                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = ctx =>
                {
                    Console.WriteLine($"JWT Failed: {ctx.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = ctx =>
                {
                    Console.WriteLine($"JWT Validated for {ctx.Principal?.Identity?.Name}");
                    return Task.CompletedTask;
                }
            };

            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                            ?? configuration["JWT:SecretKey"];
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                        ?? configuration["JWT:Issuer"];
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                        ?? configuration["JWT:Audience"];

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

        return services;
    }
}
