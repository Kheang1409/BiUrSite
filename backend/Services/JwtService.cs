using Backend.Enums;
using Backend.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User profile cannot be null.");

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey") 
                            ?? jwtSettings["SecretKey"];
            var issuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer") 
                            ?? jwtSettings["Issuer"];
            var audience = Environment.GetEnvironmentVariable("JwtSettings__Audience") 
                            ?? jwtSettings["Audience"];
            var expiryMinutes = int.TryParse(Environment.GetEnvironmentVariable("JwtSettings__ExpiryMinutes"), out var envExpiryMinutes) 
                                ? envExpiryMinutes 
                                : int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentNullException(nameof(secretKey), "JWT secret key cannot be null or empty.");
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentNullException(nameof(issuer), "JWT issuer cannot be null or empty.");
            if (string.IsNullOrEmpty(audience))
                throw new ArgumentNullException(nameof(audience), "JWT audience cannot be null or empty.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, $"{user.userId}"),
                new Claim(JwtRegisteredClaimNames.Email, user.email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.username),
                new Claim("profile", user.profile),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                new Claim(ClaimTypes.Role, user.role.ToString()) 
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }

    }
}
