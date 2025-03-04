using Backend.Enums;
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

        public string GenerateToken(int userId, string email, string username, Role role)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username), "Username cannot be null or empty.");

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Environment.GetEnvironmentVariable("JwtSettings__SecretKey") 
                        ?? jwtSettings["SecretKey"];
            var issuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer") 
                        ?? jwtSettings["Issuer"];
            var audience = Environment.GetEnvironmentVariable("JwtSettings__Audience") 
                        ?? jwtSettings["Audience"];
            var expiryMinutes = int.TryParse(Environment.GetEnvironmentVariable("JwtSettings__ExpiryMinutes"), out var envExpiryMinutes) 
                                ? envExpiryMinutes 
                                : int.Parse(jwtSettings["ExpiryMinutes"]);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, $"{userId}"),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.GivenName, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                new Claim(ClaimTypes.Role, role.ToString()) 
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
