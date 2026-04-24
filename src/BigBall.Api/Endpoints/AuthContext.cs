using System.Security.Claims;

namespace BigBall.Api.Endpoints;

internal static class AuthContext
{
    public static Guid RequireUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value
                  ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(sub, out var id))
        {
            throw new InvalidOperationException("Token sem claim 'sub' válido.");
        }
        return id;
    }
}
