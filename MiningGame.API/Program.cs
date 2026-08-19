using System.Security.Claims;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MiningGame.API.Data;
using MiningGame.API.Services;

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
        options.Authority = descopeAuthority;
        options.Audience = descopeProjectId;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = descopeAuthority,
            ValidateAudience = true,
            ValidAudience = descopeProjectId,
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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    db.Database.Migrate();
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

app.Run();

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