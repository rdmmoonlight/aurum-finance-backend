using Aurum.Api.Core.Utilities;
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
    /// Built via NpgsqlDataSourceBuilder so future enum-backed entities can
    /// register a native Postgres enum mapping here (dataSourceBuilder.MapEnum)
    /// alongside the matching modelBuilder.HasPostgresEnum call in
    /// AppDbContext.OnModelCreating.
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
