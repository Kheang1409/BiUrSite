using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backend.API.GraphQL;

public sealed class GraphQLUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<GraphQLUserContext> _logger;

    public GraphQLUserContext(IHttpContextAccessor httpContextAccessor, ILogger<GraphQLUserContext> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public GraphQLUserContext(IHttpContextAccessor httpContextAccessor)
        : this(httpContextAccessor, NullLogger<GraphQLUserContext>.Instance)
    {
    }

    public ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public Guid GetRequiredUserId()
    {
        try
        {
            var startClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray();
            var startAuth = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
            _logger.LogDebug("GetRequiredUserId invoked. Claims={Claims}, Authorization={Auth}", JsonSerializer.Serialize(startClaims), startAuth ?? "(none)");
        }
        catch { }

        var candidateClaimTypes = new[]
        {
            ClaimTypes.NameIdentifier,
            "id",
            System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub,
            ClaimTypes.Name,
            ClaimTypes.Email
        };

        foreach (var claimType in candidateClaimTypes)
        {
            var values = User.Claims.Where(c => string.Equals(c.Type, claimType, StringComparison.OrdinalIgnoreCase)).Select(c => c.Value);
            foreach (var v in values)
            {
                if (!string.IsNullOrWhiteSpace(v) && Guid.TryParse(v, out var parsed))
                    return parsed;
            }
        }

        foreach (var v in User.Claims.Select(c => c.Value))
        {
            if (!string.IsNullOrWhiteSpace(v) && Guid.TryParse(v, out var parsed))
                return parsed;
        }

        try
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray();
            var auth = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
            _logger.LogWarning("GetRequiredUserId: invalid claim. Claims={Claims}, Authorization={Auth}", JsonSerializer.Serialize(claims), auth ?? "(none)");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetRequiredUserId: failed to log claims");
        }

        throw new UnauthorizedAccessException("Invalid user id claim. Please log out and log in again.");
    }

    public string GetRequiredUsername()
    {
        var raw = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(raw))
            throw new UnauthorizedAccessException();
        return raw;
    }

    public string GetRequiredEmail()
    {
        var raw = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(raw))
            throw new UnauthorizedAccessException("Email claim not found.");
        return raw;
    }
}
