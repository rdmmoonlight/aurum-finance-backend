using NpgsqlTypes;

namespace Aurum.Api.Features.Accounting.Accounts.Entities;

/// <summary>
/// Mirrors the existing native Postgres enum "account_role" (created by the
/// original Drizzle schema) — only one label exists today, "clearing".
/// Mapped 1:1 via NpgsqlDataSourceBuilder.MapEnum in DatabaseServiceExtensions,
/// so no schema change is needed for this enum to work with EF Core.
/// </summary>
public enum AccountRole
{
    [PgName("clearing")]
    Clearing,
}
