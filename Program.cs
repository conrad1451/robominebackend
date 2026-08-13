// Program.cs
using Descope;
using DescopeScalewayApi;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// CHQ: Claude AI (Sonnet) generated file, Gemini AI modified


// 1. Configure Kestrel Port Binding (Defaults to 8080 or PORT env var)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// 2. Register Descope SDK Client in DI Container
var projectId = builder.Configuration["Descope:ProjectId"] 
    ?? Environment.GetEnvironmentVariable("DESCOPE_PROJECT_ID")
    ?? throw new InvalidOperationException("Descope Project ID is not configured.");

builder.Services.AddSingleton<IDescopeClient>(_ => new DescopeClient(new DescopeConfig
{
    ProjectId = projectId
}));

// 3. Register Authentication & Authorization Framework
builder.Services.AddAuthentication("DescopeScheme")
    .AddScheme<DescopeAuthOptions, DescopeAuthenticationHandler>("DescopeScheme", options => { });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 4. Register HTTP Middleware Pipeline
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 5. Define Endpoints
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous();

app.MapGet("/api/me", (HttpContext context) =>
{
    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    return Results.Ok(new { UserId = userId, Email = email });
})
.RequireAuthorization();

app.Run();