using Descope;
using Microsoft.AspNetCore.Http;

// CHQ: Claude AI (Sonnet) generated file

namespace DescopeScalewayApi;

public record AuthResult(bool IsAuthenticated, string? UserId, string? Email);

public static class DescopeAuth
{
    private static readonly AuthResult Anonymous = new(false, null, null);

    public static async Task<AuthResult> TryAuthenticateAsync(
        IDescopeClient descopeClient,
        HttpRequest request)
    {
        var token = ExtractBearerToken(request);
        if (string.IsNullOrEmpty(token))
        {
            return Anonymous;
        }

        try
        {
            var validated = await descopeClient.Auth.ValidateSessionAsync(token);
            return new AuthResult(
                IsAuthenticated: true,
                UserId: validated.Subject,
                Email: validated.Claims.TryGetValue("email", out var email)
                    ? email?.ToString()
                    : null);
        }
        catch (DescopeException)
        {
            return Anonymous;
        }
    }

    public static async Task<AuthResult> RequireAuthenticationAsync(
        IDescopeClient descopeClient,
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