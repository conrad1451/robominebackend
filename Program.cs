using Descope;
using DescopeScalewayApi;

// CHQ: Claude AI (Sonnet) generated file

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var projectId = Environment.GetEnvironmentVariable("DESCOPE_PROJECT_ID")
    ?? throw new InvalidOperationException("DESCOPE_PROJECT_ID environment variable is not set.");

builder.Services.AddDescopeClient(new DescopeClientOptions
{
    ProjectId = projectId,
});

var app = builder.Build();

app.MapGet("/public", async (HttpRequest request, IDescopeClient descopeClient) =>
{
    var auth = await DescopeAuth.TryAuthenticateAsync(descopeClient, request);

    var message = auth.IsAuthenticated
        ? $"Welcome back, {auth.Email ?? auth.UserId}!"
        : "Welcome, guest! Log in for a personalized view.";

    return Results.Ok(new { message, isAuthenticated = auth.IsAuthenticated });
});

app.MapGet("/profile", async (HttpRequest request, IDescopeClient descopeClient) =>
{
    try
    {
        var auth = await DescopeAuth.RequireAuthenticationAsync(descopeClient, request);
        return Results.Ok(new { userId = auth.UserId, email = auth.Email });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Json(new { error = "Login required." }, statusCode: 401);
    }
});

app.MapGet("/", () => Results.Ok(new { status = "ok" }));

app.Run();