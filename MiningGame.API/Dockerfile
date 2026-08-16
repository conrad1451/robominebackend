# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file using its relative subfolder path
COPY ["MiningGame.API/MiningGame.API.csproj", "MiningGame.API/"]
RUN dotnet restore "MiningGame.API/MiningGame.API.csproj"

# Copy full source and publish from inside the project folder
COPY . .
WORKDIR "/src/MiningGame.API"
RUN dotnet publish "MiningGame.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "MiningGame.API.dll"]