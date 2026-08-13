using Descope;
using Microsoft.AspNetCore.Http;

namespace DescopeScalewayApi;

/// <summary>
/// Represents the outcome of an optional auth check: either an authenticated
/// user, or an explicit "no/invalid token" result that callers can branch on
/// without ever throwing for the anonymous case.
/// </summary>
public record AuthResult(bool IsAuthenticated, string? UserId, string? Email);

public static class DescopeAuth
{
    private static readonly AuthResult Anonymous = new(false, null, null);

    public static async Task<AuthResult> TryAuthenticateAsync(
        DescopeClient descopeClient,
        HttpRequest request)
    {
        var token = ExtractBearerToken(request);
        if (string.IsNullOrEmpty(token))
        {
            return Anonymous;
        }

        try
        {
            var validated = await descopeClient.Auth.ValidateSession(token);
            return new AuthResult(
                IsAuthenticated: true,
                UserId: validated.Token.Subject,
                Email: validated.Token.Claims.TryGetValue("email", out var email)
                    ? email?.ToString()
                    : null);
        }
        catch (DescopeException)
        {
            return Anonymous;
        }
    }

    public static async Task<AuthResult> RequireAuthenticationAsync(
        DescopeClient descopeClient,
        HttpRequest request)
    {
        var result = await TryAuthenticateAsync(descopeClient, request);
        if (!result.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("A valid session token is required.");
        }
        return result;
    }

    private static string? ExtractBearerToken(HttpRequest request)
    {
        var header = request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return header["Bearer ".Length..].Trim();
    }
}