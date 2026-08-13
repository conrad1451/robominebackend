using Descope;
using Microsoft.AspNetCore.Http;

// CHQ: Claude AI (Sonnet) generated file

namespace DescopeScalewayApi;

public record AuthResult(bool IsAuthenticated, string? UserId, string? Email);

public static class DescopeAuth
{
    private static readonly AuthResult Anonymous = new(false, null, null);
    private const string BearerPrefix = "Bearer ";

    public static async Task<AuthResult> TryAuthenticateAsync(
        IDescopeClient descopeClient,
        HttpRequest request)
    {
        var token = ExtractBearerToken(request);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous;
        }

        try
        {
            var validated = await descopeClient.Auth.ValidateSessionAsync(token);

            string? email = null;
            if (validated.Claims.TryGetValue("email", out var emailClaim) && emailClaim is not null)
            {
                email = emailClaim.ToString();
            }

            return new AuthResult(
                IsAuthenticated: true,
                UserId: validated.Subject,
                Email: email
            );
        }
        catch (Exception) // Gracefully capture DescopeException, HTTP errors, or token parse errors
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
            throw new BadHttpRequestException("A valid Bearer session token is required.", StatusCodes.Status401Unauthorized);
        }
        return result;
    }

    private static string? ExtractBearerToken(HttpRequest request)
    {
        var header = request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return header[BearerPrefix.Length..].Trim();
    }
}