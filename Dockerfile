FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Salin file csproj langsung dari root repositori ke root /src di Docker
COPY Aurum.Api.csproj .
RUN dotnet restore Aurum.Api.csproj

# 2. Salin seluruh sisa file kode dari root repositori
COPY . .

# 3. Jalankan publish langsung ke file csproj yang ada di root
RUN dotnet publish Aurum.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Aurum.Api.dll"]

# Render provides the PORT environment variable at runtime; Kestrel is
# configured to bind to it via ASPNETCORE_URLS below.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Render injects PORT at runtime; default to 8080 for local `docker run`.
ENV PORT=8080
EXPOSE 8080
