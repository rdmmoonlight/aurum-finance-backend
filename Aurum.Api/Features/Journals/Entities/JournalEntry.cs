namespace Aurum.Api.Features.Journals.Entities;

/// <summary>
/// Maps onto the existing "journal_entries" table (created by the original
/// Drizzle schema) — no schema change. One row per debit/credit line; rows
/// sharing a GroupId form one transaction. See
/// Configurations/JournalEntryConfiguration.cs for exact column mapping.
/// </summary>
public sealed class JournalEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid PeriodId { get; set; }

    public JournalKind Kind { get; set; }

    /// <summary>Links every row belonging to the same transaction.</summary>
    public Guid GroupId { get; set; }

    /// <summary>Day-of-month (1-31); the period (my = MM-YYYY) supplies month/year.</summary>
    public int Date { get; set; }

    public string AccountRef { get; set; } = string.Empty;

    /// <summary>Denormalized at write time, same as the original schema.</summary>
    public string AccountName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Exactly one of Debit/Credit is set per row.</summary>
    public decimal? Debit { get; set; }

    public decimal? Credit { get; set; }

    public bool IsClosingTemp { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
