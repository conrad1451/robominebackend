using System.Security.Claims;

namespace MiningGame.API.Services;

public static class ClaimsPrincipalExtensions
{
    // Descope JWTs expose email either as the standard "email" claim or,
    // depending on token config, mapped onto ClaimTypes.Email. Check both.
    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue("email")
            ?? user.FindFirstValue(ClaimTypes.Email);
    }
}