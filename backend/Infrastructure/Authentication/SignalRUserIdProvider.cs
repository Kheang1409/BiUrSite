using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Infrastructure.Authentication;

/// <summary>
/// Provide user id for SignalR based on the authenticated user's claims.
/// It prefers the JWT 'sub' (subject) claim, falling back to NameIdentifier.
/// </summary>
public class SignalRUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var user = connection.User;
        if (user == null)
            return null;

        // Try 'sub' claim first (JwtRegisteredClaimNames.Sub)
        var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst("sub")?.Value
                  ?? user.Identity?.Name;

        return sub;
    }
}
