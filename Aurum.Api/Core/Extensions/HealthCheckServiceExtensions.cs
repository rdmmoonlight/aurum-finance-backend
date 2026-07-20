using Aurum.Api.Core.Utilities;

namespace Aurum.Api.Core.Extensions;

public static class HealthCheckServiceExtensions
{
    /// <summary>
    /// Registers a liveness check plus a PostgreSQL readiness check, exposed
    /// at /health for use by Render's health check and uptime monitors.
    /// </summary>
    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var rawConnectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection");

        var healthChecksBuilder = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(rawConnectionString))
        {
            var connectionString = ConnectionStringHelper.NormalizeToNpgsqlFormat(rawConnectionString);

            healthChecksBuilder.AddNpgSql(
                connectionString,
                name: "postgresql",
                tags: new[] { "ready", "db" });
        }

        return services;
    }
}
