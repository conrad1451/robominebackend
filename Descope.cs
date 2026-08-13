using Descope;
using DescopeScalewayApi;

// CHQ: Claude AI (Sonnet) generated file

var builder = WebApplication.CreateBuilder(args);

// Scaleway Containers injects the PORT env var; the container must listen
// on it. Default to 8080 for local runs.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// DESCOPE_PROJECT_ID comes from the container's environment variables,
// set as a Scaleway container env var / secret - never hardcoded.
var projectId = Environment.GetEnvironmentVariable("DESCOPE_PROJECT_ID")
    ?? throw new InvalidOperationException("DESCOPE_PROJECT_ID environment variable is not set.");

var descopeConfig = new DescopeConfig(projectId: projectId);

// Singleton: the SDK caches Descope's public keys internally, so reusing
// one client across requests avoids refetching them each time.
builder.Services.AddSingleton(new DescopeClient(descopeConfig));

var app = builder.Build();

// GET /public - never requires login. Works identically whether or not a
// token is sent; it just personalizes the response when one is valid.
// This is the "optional login" pattern: try to authenticate, fall back to
// a guest response instead of rejecting the request.
app.MapGet("/public", async (HttpRequest request, DescopeClient descopeClient) =>
{
    var auth = await DescopeAuth.TryAuthenticateAsync(descopeClient, request);

    var message = auth.IsAuthenticated
        ? $"Welcome back, {auth.Email ?? auth.UserId}!"
        : "Welcome, guest! Log in for a personalized view.";

    return Results.Ok(new
    {
        message,
        isAuthenticated = auth.IsAuthenticated,
    });
});

// GET /profile - requires a valid Descope session. Returns 401 if the token
// is missing or invalid.
app.MapGet("/profile", async (HttpRequest request, DescopeClient descopeClient) =>
{
    try
    {
        var auth = await DescopeAuth.RequireAuthenticationAsync(descopeClient, request);
        return Results.Ok(new
        {
            userId = auth.UserId,
            email = auth.Email,
        });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Json(new { error = "Login required." }, statusCode: 401);
    }
});

// Scaleway Containers health checks expect a 200 somewhere reachable.
app.MapGet("/", () => Results.Ok(new { status = "ok" }));

app.Run();