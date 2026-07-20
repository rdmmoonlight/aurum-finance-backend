using System.Diagnostics;
using Aurum.Api.Features.Security.Audit;
using Aurum.Api.Features.Security.Dtos;
using Aurum.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Features.Security;

public interface IHealthService
{
    Task<GuardianReport> GenerateGuardianReportAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Legacy alias — GET /security/health returns just the "security" domain's items under the overall guardian score.</summary>
    Task<SecurityHealthReport> GenerateHealthReportAsync(Guid userId, CancellationToken ct = default);
}

/// <summary>
/// Ported 1:1 from NestJS's HealthService (backend/src/security/health.service.ts),
/// with content adapted in two places to reflect the auth migration (see
/// this feature's README for exactly what changed and why):
///   - "Authentication"/"Supabase (Auth)" items now describe JWT auth
///     instead of a Supabase session.
///   - The required-env-vars list is DATABASE_URL + JWT_SIGNING_KEY
///     instead of DATABASE_URL + SUPABASE_URL + SUPABASE_ANON_KEY.
/// Domain weights (security 25, integrity 30, health 25, performance 10,
/// configuration 10) and the scoring formulas are unchanged.
/// </summary>
public sealed class HealthService : IHealthService
{
    private static readonly IReadOnlyDictionary<string, double> DomainWeights = new Dictionary<string, double>
    {
        ["security"] = 25,
        ["integrity"] = 30,
        ["health"] = 25,
        ["performance"] = 10,
        ["configuration"] = 10,
    };

    private readonly AppDbContext _db;
    private readonly IAuditLogService _auditLog;
    private readonly IConfiguration _configuration;

    public HealthService(AppDbContext db, IAuditLogService auditLog, IConfiguration configuration)
    {
        _db = db;
        _auditLog = auditLog;
        _configuration = configuration;
    }

    public async Task<GuardianReport> GenerateGuardianReportAsync(Guid userId, CancellationToken ct = default)
    {
        var security = BuildSecurity(userId);
        var integrity = await BuildIntegrityAsync(userId, ct);
        var health = await BuildHealthAsync(ct);
        var performance = await BuildPerformanceAsync(userId, ct);
        var configuration = BuildConfiguration();

        var domains = new List<DomainReport> { security, integrity, health, performance, configuration };

        var totalWeight = DomainWeights.Values.Sum();
        var guardianScore = Math.Round(
            domains.Sum(d => d.Score * DomainWeights.GetValueOrDefault(d.Domain, 10)) / totalWeight);

        return new GuardianReport
        {
            GuardianScore = guardianScore,
            Domains = domains,
            GeneratedAt = DateTime.UtcNow,
        };
    }

    public async Task<SecurityHealthReport> GenerateHealthReportAsync(Guid userId, CancellationToken ct = default)
    {
        var report = await GenerateGuardianReportAsync(userId, ct);
        var securityDomain = report.Domains.First(d => d.Domain == "security");

        return new SecurityHealthReport
        {
            Score = report.GuardianScore,
            Items = securityDomain.Items,
            GeneratedAt = report.GeneratedAt,
        };
    }

    private DomainReport BuildSecurity(Guid userId)
    {
        var failedCount = _auditLog.CountFailed(userId, 15);
        var recentEvents = _auditLog.ByUser(userId, 5);
        var hasRecentOk = recentEvents.Any(e => e.Result == "ok");

        var items = new List<HealthItem>
        {
            new()
            {
                Key = "auth", Label = "Authentication", Status = "ok", Value = "Active",
                Detail = "JWT bearer session valid.", Score = 100, Weight = 30,
            },
            new()
            {
                Key = "rateLimit", Label = "Rate Limit", Status = "ok", Value = "Normal",
                Detail = "No active rate limits.", Score = 100, Weight = 20,
            },
            new()
            {
                Key = "auditFailures",
                Label = "Recent Failures",
                Status = failedCount == 0 ? "ok" : failedCount < 5 ? "warn" : "error",
                Value = failedCount.ToString(),
                Detail = failedCount == 0
                    ? "No failures in the last 15 minutes."
                    : $"{failedCount} request(s) failed in the last 15 minutes.",
                Score = failedCount == 0 ? 100 : failedCount < 5 ? 70 : 30,
                Weight = 30,
            },
            new()
            {
                Key = "session",
                Label = "Session",
                Status = hasRecentOk || recentEvents.Count == 0 ? "ok" : "warn",
                Value = "Valid",
                Detail = "Session active.",
                Score = 100,
                Weight = 20,
            },
        };

        var score = DomainScore(items);
        return new DomainReport { Domain = "security", Label = "Security", Score = score, Status = DomainStatus(score), Items = items };
    }

    private async Task<DomainReport> BuildIntegrityAsync(Guid userId, CancellationToken ct)
    {
        var unbalanced = await SafeAsync(async () => await _db.Database
            .SqlQuery<int>($@"
                SELECT COUNT(*)::int FROM (
                    SELECT group_id FROM journal_entries
                    WHERE user_id = {userId} AND kind != 'CE'
                    GROUP BY group_id
                    HAVING ABS(COALESCE(SUM(debit),0) - COALESCE(SUM(credit),0)) > 0.001
                ) sub")
            .FirstAsync(ct), fallback: -1, ct);

        var orphans = await SafeAsync(async () => await _db.Database
            .SqlQuery<int>($@"
                SELECT COUNT(*)::int FROM journal_entries je
                LEFT JOIN periods p ON p.id = je.period_id
                WHERE je.user_id = {userId} AND p.id IS NULL")
            .FirstAsync(ct), fallback: -1, ct);

        var negatives = await SafeAsync(async () => await _db.Database
            .SqlQuery<int>($@"
                SELECT COUNT(*)::int FROM journal_entries
                WHERE user_id = {userId}
                  AND (COALESCE(debit, 0) < 0 OR COALESCE(credit, 0) < 0)")
            .FirstAsync(ct), fallback: -1, ct);

        var items = new List<HealthItem>
        {
            IntegrityItem("journalBalance", "Journal Balanced", unbalanced, 45),
            IntegrityItem("orphanData", "Orphan Data", orphans, 30),
            IntegrityItem("negativeBalance", "Negative Balance", negatives, 25),
        };

        var score = DomainScore(items);
        return new DomainReport { Domain = "integrity", Label = "Integrity", Score = score, Status = DomainStatus(score), Items = items };
    }

    private static HealthItem IntegrityItem(string key, string label, int count, double weight)
    {
        var failed = count < 0;
        var bad = count > 0;

        return new HealthItem
        {
            Key = key,
            Label = label,
            Status = failed ? "warn" : bad ? "error" : "ok",
            Value = failed ? "?" : count.ToString(),
            Detail = failed ? "Unable to verify." : bad ? $"{count} found." : "Clean.",
            Score = failed ? 50 : bad ? 0 : 100,
            Weight = weight,
        };
    }

    private async Task<DomainReport> BuildHealthAsync(CancellationToken ct)
    {
        var dbOk = await SafeAsync(async () =>
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1", ct);
            return true;
        }, fallback: false, ct);

        var migrationOk = await SafeAsync(async () =>
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1 FROM vw_trial_balance LIMIT 0", ct);
            return true;
        }, fallback: false, ct);

        // Adapted for the auth migration: DATABASE_URL + JWT_SIGNING_KEY,
        // not the old Supabase env vars — this backend no longer talks to
        // Supabase for anything.
        var requiredEnvVars = new[] { "DATABASE_URL", "JWT_SIGNING_KEY" };
        var missing = requiredEnvVars.Where(k => string.IsNullOrEmpty(ResolveEnv(k))).ToList();

        var items = new List<HealthItem>
        {
            new()
            {
                Key = "database", Label = "Neon (Database)",
                Status = dbOk ? "ok" : "error", Value = dbOk ? "Connected" : "Error",
                Detail = dbOk ? "Database connection active." : "Unable to connect to Neon.",
                Score = dbOk ? 100 : 0, Weight = 40,
            },
            new()
            {
                Key = "authProvider", Label = "Authentication Provider",
                Status = "ok", Value = "JWT (local)",
                Detail = "Local JWT issuing/validation active (no external auth provider).",
                Score = 100, Weight = 25,
            },
            new()
            {
                Key = "migration", Label = "Migration",
                Status = migrationOk ? "ok" : "warn", Value = migrationOk ? "Applied" : "Pending",
                Detail = migrationOk ? "All migrations applied." : "Reporting views are missing or not migrated.",
                Score = migrationOk ? 100 : 50, Weight = 20,
            },
            new()
            {
                Key = "environment", Label = "Environment",
                Status = missing.Count == 0 ? "ok" : "error",
                Value = missing.Count == 0 ? "Complete" : $"{missing.Count} missing",
                Detail = missing.Count == 0 ? "All env vars present." : $"Missing: {string.Join(", ", missing)}",
                Score = missing.Count == 0 ? 100 : 0, Weight = 15,
            },
        };

        var score = DomainScore(items);
        return new DomainReport { Domain = "health", Label = "Health", Score = score, Status = DomainStatus(score), Items = items };
    }

    private async Task<DomainReport> BuildPerformanceAsync(Guid userId, CancellationToken ct)
    {
        static string LatencyStatus(double ms) => ms < 100 ? "ok" : ms < 300 ? "warn" : "error";

        var pingSw = Stopwatch.StartNew();
        await SafeAsync(async () =>
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1", ct);
            return true;
        }, fallback: false, ct);
        pingSw.Stop();
        var pingMs = pingSw.Elapsed.TotalMilliseconds;

        var querySw = Stopwatch.StartNew();
        await SafeAsync(async () => await _db.JournalEntries.CountAsync(j => j.UserId == userId, ct), fallback: 0, ct);
        querySw.Stop();
        var queryMs = querySw.Elapsed.TotalMilliseconds;

        var items = new List<HealthItem>
        {
            new()
            {
                Key = "dbLatency", Label = "DB Latency",
                Status = LatencyStatus(pingMs), Value = $"{pingMs:F0} ms",
                Detail = pingMs < 100 ? "Normal latency." : pingMs < 300 ? "Moderate latency." : "High latency.",
                Score = pingMs < 100 ? 100 : pingMs < 300 ? 70 : 30, Weight = 50,
            },
            new()
            {
                Key = "queryTime", Label = "Sample Query",
                Status = LatencyStatus(queryMs), Value = $"{queryMs:F0} ms",
                Detail = $"COUNT query on user data: {queryMs:F0}ms.",
                Score = queryMs < 100 ? 100 : queryMs < 300 ? 70 : 30, Weight = 50,
            },
        };

        var score = DomainScore(items);
        return new DomainReport { Domain = "performance", Label = "Performance", Score = score, Status = DomainStatus(score), Items = items };
    }

    private DomainReport BuildConfiguration()
    {
        var appVersion = _configuration["AppVersion"];
        var timezone = ResolveEnv("TZ") ?? "Asia/Jakarta";

        // Adapted for the auth migration — see class remarks.
        var envMap = new Dictionary<string, string>
        {
            ["DATABASE_URL"] = "Database URL",
            ["JWT_SIGNING_KEY"] = "JWT Signing Key",
        };

        var items = envMap.Select(kv =>
        {
            var exists = !string.IsNullOrEmpty(ResolveEnv(kv.Key));
            return new HealthItem
            {
                Key = kv.Key,
                Label = kv.Value,
                Status = exists ? "ok" : "error",
                Value = kv.Key.Contains("KEY") ? "••••••••" : exists ? "Set" : "Missing",
                Detail = exists ? "Available." : "Env var not found.",
                Score = exists ? 100 : 0,
                Weight = 100.0 / envMap.Count,
            };
        }).ToList();

        items.Add(new HealthItem
        {
            Key = "timezone", Label = "Timezone", Status = "ok", Value = timezone,
            Detail = $"Server timezone: {timezone}.", Score = 100, Weight = 0,
        });

        items.Add(new HealthItem
        {
            Key = "appVersion", Label = "App Version",
            Status = appVersion is not null ? "ok" : "error",
            Value = appVersion is not null ? $"v{appVersion}" : "Missing",
            Detail = appVersion is not null ? "Application version active." : "AppVersion not configured.",
            Score = appVersion is not null ? 100 : 0, Weight = 0,
        });

        var score = DomainScore(items.Where(i => i.Weight > 0).ToList());
        return new DomainReport { Domain = "configuration", Label = "Configuration", Score = score, Status = DomainStatus(score), Items = items };
    }

    private static double DomainScore(IReadOnlyList<HealthItem> items)
    {
        var totalWeight = items.Sum(i => i.Weight);
        if (totalWeight == 0)
        {
            return 100;
        }

        return Math.Round(items.Sum(i => i.Score * i.Weight) / totalWeight);
    }

    private static string DomainStatus(double score) => score >= 90 ? "ok" : score >= 70 ? "warn" : "error";

    private static string? ResolveEnv(string key) => Environment.GetEnvironmentVariable(key);

    private static async Task<T> SafeAsync<T>(Func<Task<T>> fn, T fallback, CancellationToken ct)
    {
        try
        {
            return await fn();
        }
        catch
        {
            return fallback;
        }
    }
}
