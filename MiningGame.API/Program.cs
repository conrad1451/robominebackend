using System.Linq;
using System.Security.Claims;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MiningGame.API.Data;
using MiningGame.API.Services;

// MiningGame.API/Program.cs

// CHQ: Gemini AI: Fix WebApplication.CreateBuilder syntax
var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Database Setup
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    var databaseUrl = builder.Configuration["DATABASE_URL"];
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        connectionString = ConvertPostgresUrlToConnectionString(databaseUrl);
    }
}

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' or 'DATABASE_URL' was not found.");
}

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Descope Authentication Setup ---
var descopeProjectId = builder.Configuration["Descope:ProjectId"]
    ?? builder.Configuration["DESCOPE_PROJECT_ID"]
    ?? throw new InvalidOperationException("Descope Project ID is not configured.");

var descopeAuthority = $"https://api.descope.com/{descopeProjectId}";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Authority is only used to fetch the JWKS (signing keys) for
        // signature verification - this endpoint is always available at this
        // path regardless of JWT Template configuration, so it's safe as-is.
        options.Authority = descopeAuthority;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            // Descope's default session tokens set `iss` to the bare project
            // ID (e.g. "P2abc..."). The `iss` only becomes the full
            // "https://api.descope.com/<projectId>" URL if the project has
            // the "AWS API Gateway" JWT Template enabled in the Descope
            // dashboard (Project Settings > JWT Templates). Since that's a
            // dashboard setting this code can't see, accept both forms so
            // auth doesn't depend on it being configured a specific way.
            IssuerValidator = (issuer, _, _) =>
                issuer == descopeProjectId || issuer == descopeAuthority
                    ? issuer
                    : throw new SecurityTokenInvalidIssuerException($"Invalid issuer '{issuer}'."),
            // ValidateAudience = true,
            ValidateAudience = false,
            // Audience intentionally not validated: this API is the sole consumer of
            // tokens from this Descope project, and ValidateIssuer already pins tokens
            // to this project. Default Descope session tokens don't include an `aud`
            // claim unless an Inbound App is configured in the Descope console. Note:
            // a custom AudienceValidator delegate overrides ValidateAudience entirely
            // if both are set, so it must be removed (not just disabled) here.            
            // AudienceValidator = (audiences, _, _) =>
            //     audiences.Any(a => a == descopeProjectId),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(";") ?? new[] { "http://localhost:5173" })
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Services
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<MiningService>();
builder.Services.AddScoped<RobotService>();
builder.Services.AddScoped<ProcessingService>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Serializes enums (MineType, MaterialType, RobotType) as snake_case
    // strings ("rare_earth") to match the frontend's TS string literal types,
    // instead of the default integer representation.
    options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.SnakeCaseLower));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CHQ: Gemini AI: refactored to invoke the async seeding method:
var app = builder.Build();

// Migrations & Seeding Pipeline
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    
    // Applies any pending EF Core migrations to PostgreSQL asynchronously
    await db.Database.MigrateAsync();

    // Seeds initial static data asynchronously
    await DbInitializer.SeedDataAsync(db); 
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

static string ConvertPostgresUrlToConnectionString(string url)
{
    var uri = new Uri(url);
    
    // Unescape encoded characters (e.g., %40 -> @) in username/password
    var rawUserInfo = uri.UserInfo.Split(':', 2);
    var username = rawUserInfo.Length > 0 ? Uri.UnescapeDataString(rawUserInfo[0]) : "";
    var password = rawUserInfo.Length > 1 ? Uri.UnescapeDataString(rawUserInfo[1]) : "";
    
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;Pooling=true;";
}