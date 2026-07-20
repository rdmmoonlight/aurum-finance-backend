namespace Aurum.Api.Features.Security.Audit;

/// <summary>
/// One recorded request outcome. Category is one of "AUTH", "DATA",
/// "COMMAND", "SECURITY"; Result is one of "ok", "failed", "blocked" — kept
/// as plain strings (not enums) since these are just log tags, same as the
/// original TypeScript string-union types.
/// </summary>
public sealed class AuditEvent
{
    public string RequestId { get; init; } = string.Empty;

    public Guid UserId { get; init; }

    public string Ip { get; init; } = string.Empty;

    public string Endpoint { get; init; } = string.Empty;

    public string Method { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public string Result { get; init; } = string.Empty;

    public string? Detail { get; init; }

    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    public double? DurationMs { get; init; }
}

public sealed class AuditRecordInput
{
    public required string RequestId { get; init; }

    public required Guid UserId { get; init; }

    public required string Ip { get; init; }

    public required string Endpoint { get; init; }

    public required string Method { get; init; }

    public required string Category { get; init; }

    public required string Action { get; init; }

    public required string Result { get; init; }

    public string? Detail { get; init; }

    public DateTime? StartedAtUtc { get; init; }
}
