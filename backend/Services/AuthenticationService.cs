using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System.Security.Claims;

namespace Backend.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        public void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            // Retrieve JWT settings from environment variables or default values
            var secretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey") 
                        ??configuration["JwtSettings:SecretKey"];
            var issuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer") 
                        ?? configuration["JwtSettings:Issuer"];
            var audience = Environment.GetEnvironmentVariable("JwtSettings__Audience") 
                        ?? configuration["JwtSettings:Audience"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("JWT settings (SecretKey, Issuer, Audience) are not properly configured.");
            }

            var key = Encoding.ASCII.GetBytes(secretKey);

            // Add authentication and JWT Bearer token validation
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        RoleClaimType = ClaimTypes.Role
                    };
                });
        }
    }
}