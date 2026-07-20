namespace Aurum.Api.Features.Accounting.Periods.Dtos;

public sealed class PeriodDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string My { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public decimal SeedCash { get; init; }

    public decimal SeedBank { get; init; }

    public DateTime? ClosedAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
