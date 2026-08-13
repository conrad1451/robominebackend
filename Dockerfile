# CHQ: Claude AI (Sonnet) generated file, Gemini AI modified

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["DescopeScalewayApi.csproj", "./"]
RUN dotnet restore "DescopeScalewayApi.csproj"

COPY . .
RUN dotnet publish "DescopeScalewayApi.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Bind Kestrel to HTTP port 8080 across all network interfaces
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "DescopeScalewayApi.dll"]