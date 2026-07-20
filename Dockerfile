# ---- Build stage ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (leveraging Docker layer caching)
COPY Aurum.Api.csproj .
RUN dotnet restore Aurum.Api.csproj

# Copy the rest of the source and publish
COPY . .
RUN dotnet publish Aurum.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage --------------------------------------------------------
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
