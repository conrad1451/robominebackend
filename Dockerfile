# CHQ: Claude AI (Sonnet) generated file

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DescopeScalewayApi.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Scaleway Containers sets PORT at runtime; Program.cs reads it and binds
# accordingly. 8080 is just the local/default fallback.
EXPOSE 8080

ENTRYPOINT ["dotnet", "DescopeScalewayApi.dll"]