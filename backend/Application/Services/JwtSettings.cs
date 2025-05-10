namespace Bacnkend.Application.Services
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = default!;
        public string Issuer    { get; set; } = default!;
        public string Audience  { get; set; } = default!;
        public int ExpiresInMinutes { get; set; }
    }
}
