FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore dependencies first for layer caching
COPY PeralAPI/PeralAPI.csproj PeralAPI/
RUN dotnet restore PeralAPI/PeralAPI.csproj

COPY . .
RUN dotnet publish PeralAPI/PeralAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Railway and Render inject $PORT at runtime; default to 8080.
# Set ASPNETCORE_HTTP_PORTS in your service dashboard to override.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PeralAPI.dll"]
