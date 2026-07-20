using System.Diagnostics;
using Aurum.Api.Features.Security.Audit;

namespace Aurum.Api.Core.Middleware;

/// <summary>
/// Mirrors NestJS's global AuditInterceptor: records every request's
/// outcome into IAuditLogService under category "DATA". Placed after
/// authentication/authorization in the pipeline so User.Identity is
/// populated when available, but does not require it — anonymous
/// endpoints (register/login) are logged with an empty userId.
///
/// Not yet ported: the original SupabaseAuthMiddleware separately logged
/// missing/invalid-token attempts under category "AUTH". This middleware
/// only covers the AuditInterceptor half — see the Security feature's
/// README for that gap.
/// </summary>
public sealed class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLog)
    {
        var startedAt = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        var userIdClaim = context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        var userId = Guid.TryParse(userIdClaim, out var parsed) ? parsed : Guid.Empty;

        var statusCode = context.Response.StatusCode;
        var result = statusCode switch
        {
            401 or 403 => "blocked",
            >= 400 => "failed",
            _ => "ok",
        };

        auditLog.Record(new AuditRecordInput
        {
            RequestId = context.TraceIdentifier,
            UserId = userId,
            Ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            Endpoint = context.Request.Path.ToString(),
            Method = context.Request.Method,
            Category = "DATA",
            Action = $"{context.Request.Method} {context.Request.Path}",
            Result = result,
            Detail = statusCode >= 400 ? $"HTTP {statusCode}" : null,
            StartedAtUtc = startedAt,
        });
    }
}

public static class AuditLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<AuditLoggingMiddleware>();
}
