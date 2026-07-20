using Aurum.Api.Features.Journals;

namespace Aurum.Api.Core.Extensions;

public static class JournalsServiceExtensions
{
    public static IServiceCollection AddJournalsServices(this IServiceCollection services)
    {
        services.AddScoped<IJournalEntriesService, JournalEntriesService>();

        return services;
    }
}
