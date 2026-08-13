// DescopeAuthenticationHandler.cs
using System.Security.Claims;
using System.Text.Encodings.Web;
using Descope;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace DescopeScalewayApi;

// CHQ: Gemini AI generated file


public class DescopeAuthOptions : AuthenticationSchemeOptions { }

public class DescopeAuthenticationHandler : AuthenticationHandler<DescopeAuthOptions>
{
    private readonly IDescopeClient _descopeClient;
    private const string BearerPrefix = "Bearer ";

    public DescopeAuthenticationHandler(
        IOptionsMonitor<DescopeAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IDescopeClient descopeClient) 
        : base(options, logger, encoder)
    {
        _descopeClient = descopeClient;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? authorizationHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader) || 
            !authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authorizationHeader[BearerPrefix.Length..].Trim();

        try
        {
            var validated = await _descopeClient.Auth.ValidateSessionAsync(token);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, validated.Subject)
            };

            if (validated.Claims.TryGetValue("email", out var emailClaim) && emailClaim is not null)
            {
                claims.Add(new Claim(ClaimTypes.Email, emailClaim.ToString()!));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Descope token validation failed.");
            return AuthenticateResult.Fail("Invalid or expired session token.");
        }
    }
}