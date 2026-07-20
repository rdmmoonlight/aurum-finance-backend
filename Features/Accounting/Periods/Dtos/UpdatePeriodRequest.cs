using Aurum.Api.Core.Shared;

namespace Aurum.Api.Features.Accounting.Periods.Dtos;

public sealed class UpdatePeriodRequest
{
    public Optional<string> My { get; init; }

    public Optional<decimal> SeedCash { get; init; }

    public Optional<decimal> SeedBank { get; init; }

    public Optional<string> Status { get; init; }

    /// <summary>ISO 8601 timestamp.</summary>
    public Optional<string> ClosedAt { get; init; }
}
