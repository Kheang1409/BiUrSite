using Backend.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Infrastructure.Authentication;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string email, string username, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim("id", userId.ToString()),

            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),

            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Role, role)
        };

        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                        ?? _configuration["JWT:SecretKey"]
                        ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                     ?? _configuration["JWT:Issuer"]
                     ?? throw new InvalidOperationException("JWT Issuer is not configured.");

        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                       ?? _configuration["JWT:Audience"]
                       ?? throw new InvalidOperationException("JWT Audience is not configured.");

        var expiresInMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")
                            ?? _configuration["JWT:ExpiresInMinutes"]
                            ?? throw new InvalidOperationException("JWT Expires Minutes is not configured."));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
