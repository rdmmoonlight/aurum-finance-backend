using Aurum.Api.Features.Ledger;

namespace Aurum.Api.Core.Extensions;

public static class LedgerServiceExtensions
{
    public static IServiceCollection AddLedgerServices(this IServiceCollection services)
    {
        services.AddScoped<ILedgerService, LedgerService>();

        return services;
    }
}
