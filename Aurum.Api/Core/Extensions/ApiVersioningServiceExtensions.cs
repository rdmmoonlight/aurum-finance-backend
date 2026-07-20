using Asp.Versioning;

namespace Aurum.Api.Core.Extensions;

public static class ApiVersioningServiceExtensions
{
    /// <summary>
    /// Prepares the application for API versioning (e.g. /api/v1/...).
    /// Only v1 is exposed today; additional versions can be introduced
    /// per-controller as the API evolves.
    /// </summary>
    public static IServiceCollection AddAppApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
