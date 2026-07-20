using Aurum.Api.Features.Reports;

namespace Aurum.Api.Core.Extensions;

public static class ReportsServiceExtensions
{
    public static IServiceCollection AddReportsServices(this IServiceCollection services)
    {
        services.AddScoped<IAnnualSummaryService, AnnualSummaryService>();

        return services;
    }
}
