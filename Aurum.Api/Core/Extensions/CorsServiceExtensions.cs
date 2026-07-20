namespace Aurum.Api.Core.Extensions;

public static class CorsServiceExtensions
{
    public const string PolicyName = "AurumCorsPolicy";

    /// <summary>
    /// Registers a CORS policy driven by the Cors:AllowedOrigins configuration
    /// section (or the ALLOWED_ORIGINS environment variable, comma-separated),
    /// so the frontend origin(s) can be changed per environment without a
    /// code change.
    /// </summary>
    public static IServiceCollection AddAppCors(this IServiceCollection services, IConfiguration configuration)
    {
        var originsFromEnv = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");

        var allowedOrigins = !string.IsNullOrWhiteSpace(originsFromEnv)
            ? originsFromEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    // No origins configured: reject all cross-origin requests
                    // rather than silently allowing everything.
                    return;
                }

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
