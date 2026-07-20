namespace Aurum.Api.Features.Security.Dtos;

/// <summary>"ok", "warn", or "error".</summary>
public sealed class HealthItem
{
    public string Key { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string? Value { get; init; }

    public string? Detail { get; init; }

    public double Score { get; init; }

    public double Weight { get; init; }
}

public sealed class DomainReport
{
    public string Domain { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public double Score { get; init; }

    public string Status { get; init; } = string.Empty;

    public List<HealthItem> Items { get; init; } = new();
}

public sealed class GuardianReport
{
    public double GuardianScore { get; init; }

    public List<DomainReport> Domains { get; init; } = new();

    public DateTime GeneratedAt { get; init; }
}

/// <summary>Legacy alias shape — GET /api/security/health.</summary>
public sealed class SecurityHealthReport
{
    public double Score { get; init; }

    public List<HealthItem> Items { get; init; } = new();

    public DateTime GeneratedAt { get; init; }
}

public sealed class AuditLogResponse
{
    public List<Audit.AuditEvent> Events { get; init; } = new();

    public int FailedCount { get; init; }
}
