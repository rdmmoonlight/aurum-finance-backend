using NpgsqlTypes;

namespace Aurum.Api.Features.Journals.Entities;

/// <summary>
/// Mirrors the existing native Postgres enum "journal_kind": General
/// Journal, Adjusting Entries, Closing Entries. Mapped via
/// NpgsqlDataSourceBuilder.MapEnum in DatabaseServiceExtensions.
/// </summary>
public enum JournalKind
{
    /// <summary>General Journal.</summary>
    [PgName("GJ")]
    Gj,

    /// <summary>Adjusting Entries.</summary>
    [PgName("AE")]
    Ae,

    /// <summary>Closing Entries.</summary>
    [PgName("CE")]
    Ce,
}
