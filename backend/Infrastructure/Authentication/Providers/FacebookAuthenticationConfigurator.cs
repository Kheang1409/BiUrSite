using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure.Authentication.Providers;

public class FacebookAuthenticationConfigurator : IAuthenticationProviderConfigurator
{
    public void Configure(AuthenticationBuilder builder, IConfiguration configuration)
    {
        var appId = Environment.GetEnvironmentVariable("FACEBOOK_APP_ID")
                    ?? configuration["Authentication:Facebook:AppId"]
                    ?? throw new InvalidOperationException("Authentication:Facebook:AppId is not configured.");
        var appSecret = Environment.GetEnvironmentVariable("FACEBOOK_APP_SECRET")
                        ?? configuration["Authentication:Facebook:AppSecret"]
                        ?? throw new InvalidOperationException("Authentication:Facebook:AppSecret is not configured.");

        builder.AddFacebook(options =>
        {
            options.AppId = appId;
            options.AppSecret = appSecret;
            options.CallbackPath = "/signin-facebook";
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });
    }
}
