using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Aurum.Api.Core.Extensions;

public static class RateLimitingServiceExtensions
{
    public const string DefaultPolicy = "AurumDefaultPolicy";

    /// <summary>
    /// Registers a global fixed-window rate limiter using ASP.NET Core's
    /// built-in middleware. Limits are configurable via RateLimiting:*
    /// so they can be tuned per environment without a code change.
    /// </summary>
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:PermitLimit", 100);
        var windowSeconds = configuration.GetValue("RateLimiting:WindowSeconds", 60);
        var queueLimit = configuration.GetValue("RateLimiting:QueueLimit", 0);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                        QueueLimit = queueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));
        });

        return services;
    }
}
