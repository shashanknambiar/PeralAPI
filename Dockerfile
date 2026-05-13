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

# Debian 12 ships OpenSSL 3 with SECLEVEL=2 by default, which filters out
# cipher suites required by MongoDB Atlas and causes SSL_R_TLSV1_ALERT_INTERNAL_ERROR.
# Dropping to SECLEVEL=1 restores compatibility while keeping TLS 1.2+.
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf || true

# Railway and Render inject $PORT at runtime; default to 8080.
# Set ASPNETCORE_HTTP_PORTS in your service dashboard to override.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PeralAPI.dll"]
