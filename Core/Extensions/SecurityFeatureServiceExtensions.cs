using Aurum.Api.Features.Security;
using Aurum.Api.Features.Security.Audit;

namespace Aurum.Api.Core.Extensions;

public static class SecurityFeatureServiceExtensions
{
    /// <summary>
    /// IAuditLogService is a singleton — it's an in-memory ring buffer that
    /// must be the same instance across every request, exactly like Nest's
    /// default-singleton @Injectable() scope for AuditLogService.
    /// </summary>
    public static IServiceCollection AddSecurityFeatureServices(this IServiceCollection services)
    {
        services.AddSingleton<IAuditLogService, AuditLogService>();
        services.AddScoped<IHealthService, HealthService>();

        return services;
    }
}
