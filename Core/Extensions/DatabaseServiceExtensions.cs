using Aurum.Api.Core.Utilities;
using Aurum.Api.Features.Accounting.Accounts.Entities;
using Aurum.Api.Features.Accounting.Periods.Entities;
using Aurum.Api.Features.Journals.Entities;
using Aurum.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Aurum.Api.Core.Extensions;

public static class DatabaseServiceExtensions
{
    /// <summary>
    /// Registers <see cref="AppDbContext"/> against PostgreSQL. The connection
    /// string is resolved, in order of priority, from:
    /// 1. The DATABASE_URL environment variable (Render/Neon convention)
    /// 2. ConnectionStrings:DefaultConnection in configuration
    ///
    /// Built via NpgsqlDataSourceBuilder (rather than a plain connection
    /// string passed to UseNpgsql) so the native Postgres enum types created
    /// by the original Drizzle schema map directly onto C# enums instead of
    /// EF Core/Npgsql seeing them as opaque/unknown types. Add one MapEnum
    /// call here for every enum-backed entity as more features are migrated.
    /// </summary>
    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var rawConnectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No database connection string configured. Set the DATABASE_URL environment variable " +
                "or ConnectionStrings:DefaultConnection in appsettings.json.");

        var connectionString = ConnectionStringHelper.NormalizeToNpgsqlFormat(rawConnectionString);

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<AccountRole>("account_role");
        dataSourceBuilder.MapEnum<PeriodStatus>("period_status");
        dataSourceBuilder.MapEnum<JournalKind>("journal_kind");
        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        return services;
    }
}
