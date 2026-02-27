using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Infrastructure.Authentication;
public class SignalRUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var user = connection.User;
        if (user == null)
            return null;

        var idClaim = user.FindFirst("id")?.Value;
        if (!string.IsNullOrEmpty(idClaim)) return idClaim;

        var nameId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(nameId)) return nameId;

        var sub = user.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(sub)) return sub;

        return user.Identity?.Name;
    }
}
