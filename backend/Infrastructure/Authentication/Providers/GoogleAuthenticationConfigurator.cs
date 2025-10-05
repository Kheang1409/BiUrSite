using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure.Authentication.Providers;

public class GoogleAuthenticationConfigurator : IAuthenticationProviderConfigurator
{
    public void Configure(AuthenticationBuilder builder, IConfiguration configuration)
    {
        var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
                       ?? configuration["Authentication:Google:ClientId"]
                       ?? throw new InvalidOperationException("Authentication:Google:ClientId is not configured.");
        var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
                           ?? configuration["Authentication:Google:ClientSecret"]
                           ?? throw new InvalidOperationException("Authentication:Google:ClientSecret is not configured.");

        builder.AddGoogle(options =>
        {
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.CallbackPath = "/users/signin-google";
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });
    }
}
