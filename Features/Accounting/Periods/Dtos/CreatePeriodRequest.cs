namespace Aurum.Api.Features.Accounting.Periods.Dtos;

public sealed class CreatePeriodRequest
{
    /// <summary>MM-YYYY, e.g. "06-2026" — matches periods.my varchar(7).</summary>
    public string My { get; init; } = string.Empty;

    public decimal SeedCash { get; init; }

    public decimal SeedBank { get; init; }

    public string? Status { get; init; }
}
