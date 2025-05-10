using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Application.Services;
using Bacnkend.Application.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(
            IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public int TokenValidityInMinutes => _settings.ExpiresInMinutes;

        public string CreateToken(int userId, string email, string userName)
        {
            try
            {
                var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, email),
                    new Claim(JwtRegisteredClaimNames.GivenName, userName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes),
                    signingCredentials: creds
                );
                return new JwtSecurityTokenHandler().WriteToken(token);
            } 
            catch (Exception)
            {
                throw;
            }
        }
    }
}
