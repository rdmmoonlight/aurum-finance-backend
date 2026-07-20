using Aurum.Api.Features.BankAccount;
using Aurum.Api.Infrastructure.External.Bri;

namespace Aurum.Api.Core.Extensions;

public static class BankAccountServiceExtensions
{
    /// <summary>
    /// IBriClient is a singleton — it caches an OAuth2 token in memory across
    /// requests, same as Nest's default-singleton @Injectable() scope. It
    /// depends on IHttpClientFactory (a named "Bri" client) rather than
    /// holding its own HttpClient, to avoid socket exhaustion.
    /// </summary>
    public static IServiceCollection AddBankAccountServices(this IServiceCollection services)
    {
        services.AddHttpClient("Bri");
        services.AddSingleton<IBriClient, BriClient>();
        services.AddScoped<IBankAccountService, BankAccountService>();

        return services;
    }
}
