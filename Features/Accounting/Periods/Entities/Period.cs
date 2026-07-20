namespace Aurum.Api.Features.Accounting.Periods.Entities;

/// <summary>
/// Maps onto the existing "periods" table (created by the original
/// Drizzle/NestJS schema) — no schema change. See
/// Configurations/PeriodConfiguration.cs for exact column mapping.
/// </summary>
public sealed class Period
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    /// <summary>MM-YYYY, e.g. "06-2026".</summary>
    public string My { get; set; } = string.Empty;

    public PeriodStatus Status { get; set; } = PeriodStatus.Open;

    public decimal SeedCash { get; set; }

    public decimal SeedBank { get; set; }

    public DateTime? ClosedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
