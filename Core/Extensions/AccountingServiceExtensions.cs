using Aurum.Api.Features.Accounting.Accounts;
using Aurum.Api.Features.Accounting.Periods;

namespace Aurum.Api.Core.Extensions;

public static class AccountingServiceExtensions
{
    public static IServiceCollection AddAccountingServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountsService, AccountsService>();
        services.AddScoped<IPeriodsService, PeriodsService>();

        return services;
    }
}
