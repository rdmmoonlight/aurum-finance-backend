namespace Aurum.Api.Features.Security.Audit;

public interface IAuditLogService
{
    void Record(AuditRecordInput input);

    /// <summary>Most recent events across every user — not currently exposed by an endpoint, kept for parity with the original.</summary>
    IReadOnlyList<AuditEvent> Recent(int limit = 50);

    IReadOnlyList<AuditEvent> ByUser(Guid userId, int limit = 20);

    int CountFailed(Guid userId, int windowMinutes = 15);
}

/// <summary>
/// Ported 1:1 from NestJS's AuditLogService (itself a port of
/// apps/web/lib/security/audit.ts). In-memory ring buffer (max 500
/// events, per running instance) — not persisted to Postgres. Data resets
/// on a cold start/instance restart; that's a known limitation carried
/// over deliberately, not a bug.
///
/// Registered as a singleton (see SecurityServiceExtensions) so the same
/// buffer is shared across every request — the direct equivalent of
/// Nest's default-singleton @Injectable() scope. Because ASP.NET handles
/// requests on a thread pool (unlike Node's single-threaded event loop),
/// a lock around the buffer was added here — the one deliberate change
/// from the original, needed for correctness under real concurrency.
/// </summary>
public sealed class AuditLogService : IAuditLogService
{
    private const int MaxBuffer = 500;

    private readonly List<AuditEvent> _buffer = new();
    private readonly object _lock = new();

    public void Record(AuditRecordInput input)
    {
        var timestamp = DateTime.UtcNow;

        var evt = new AuditEvent
        {
            RequestId = input.RequestId,
            UserId = input.UserId,
            Ip = input.Ip,
            Endpoint = input.Endpoint,
            Method = input.Method,
            Category = input.Category,
            Action = input.Action,
            Result = input.Result,
            Detail = input.Detail,
            TimestampUtc = timestamp,
            DurationMs = input.StartedAtUtc.HasValue ? (timestamp - input.StartedAtUtc.Value).TotalMilliseconds : null,
        };

        lock (_lock)
        {
            _buffer.Insert(0, evt);
            if (_buffer.Count > MaxBuffer)
            {
                _buffer.RemoveRange(MaxBuffer, _buffer.Count - MaxBuffer);
            }
        }

        var prefix = evt.Result switch
        {
            "ok" => "✓",
            "blocked" => "⊘",
            _ => "✗",
        };

        Console.WriteLine(
            $"[AUDIT] {prefix} {evt.Category}:{evt.Action} requestId={evt.RequestId} " +
            $"userId={ShortId(evt.UserId)} ip={evt.Ip} result={evt.Result} detail={evt.Detail} ms={evt.DurationMs}");
    }

    public IReadOnlyList<AuditEvent> Recent(int limit = 50)
    {
        lock (_lock)
        {
            return _buffer.Take(limit).ToList();
        }
    }

    public IReadOnlyList<AuditEvent> ByUser(Guid userId, int limit = 20)
    {
        lock (_lock)
        {
            return _buffer.Where(e => e.UserId == userId).Take(limit).ToList();
        }
    }

    public int CountFailed(Guid userId, int windowMinutes = 15)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-windowMinutes);

        lock (_lock)
        {
            return _buffer.Count(e => e.UserId == userId && e.Result == "failed" && e.TimestampUtc > cutoff);
        }
    }

    private static string ShortId(Guid userId) => userId.ToString()[..8] + "…";
}
