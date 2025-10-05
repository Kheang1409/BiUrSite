using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Authentication;

public interface IAuthenticationProviderConfigurator
{
    void Configure(AuthenticationBuilder builder, IConfiguration configuration);
}
