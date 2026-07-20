using Aurum.Api.Features.Security.Audit;
using Aurum.Api.Features.Security.Dtos;
using Aurum.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Features.Security;

[ApiController]
[Authorize]
[Route("api/security")]
public sealed class SecurityController : ControllerBase
{
    private readonly IHealthService _healthService;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentUserService _currentUser;

    public SecurityController(IHealthService healthService, IAuditLogService auditLog, ICurrentUserService currentUser)
    {
        _healthService = healthService;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    [HttpGet("guardian")]
    public async Task<ActionResult<GuardianReport>> GetGuardianReport(CancellationToken ct)
    {
        return Ok(await _healthService.GenerateGuardianReportAsync(_currentUser.UserId, ct));
    }

    [HttpGet("health")]
    public async Task<ActionResult<SecurityHealthReport>> GetHealthReport(CancellationToken ct)
    {
        return Ok(await _healthService.GenerateHealthReportAsync(_currentUser.UserId, ct));
    }

    [HttpGet("audit-log")]
    public ActionResult<AuditLogResponse> GetAuditLog()
    {
        var userId = _currentUser.UserId;

        return Ok(new AuditLogResponse
        {
            Events = _auditLog.ByUser(userId, 50).ToList(),
            FailedCount = _auditLog.CountFailed(userId, 15),
        });
    }
}
