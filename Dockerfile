# CHQ: Claude AI (Sonnet) generated file, Gemini AI modified

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore project layers independently to optimize Docker layer caching
COPY ["DescopeScalewayApi.csproj", "./"]
RUN dotnet restore "DescopeScalewayApi.csproj"

# Copy remaining source files and publish
COPY . .
RUN dotnet publish "DescopeScalewayApi.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy compiled output from build stage
COPY --from=build /app/publish .

# Scaleway Containers injects $PORT at runtime (defaults to 8080 in .NET 8)
EXPOSE 8080

# Enforce non-root execution (built-in .NET 8 feature)
USER $APP_UID

ENTRYPOINT ["dotnet", "DescopeScalewayApi.dll"]