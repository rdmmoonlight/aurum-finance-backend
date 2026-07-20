using NpgsqlTypes;

namespace Aurum.Api.Features.Accounting.Periods.Entities;

/// <summary>
/// Mirrors the existing native Postgres enum "period_status" — open,
/// closing, closed. Mapped via NpgsqlDataSourceBuilder.MapEnum in
/// DatabaseServiceExtensions; no schema change needed.
/// </summary>
public enum PeriodStatus
{
    [PgName("open")]
    Open,

    [PgName("closing")]
    Closing,

    [PgName("closed")]
    Closed,
}
