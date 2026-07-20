using Aurum.Api.Features.Ledger.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Features.Ledger;

/// <summary>
/// New endpoints — see LedgerService's remarks for why there's no Nest
/// contract to mirror here specifically. Routes and query parameter naming
/// follow the same conventions as every other migrated feature (periodId
/// as a query param, flat JSON array responses, [Authorize] + [FromQuery]).
/// </summary>
[ApiController]
[Authorize]
[Route("api/ledger")]
public sealed class LedgerController : ControllerBase
{
    private readonly ILedgerService _ledgerService;

    public LedgerController(ILedgerService ledgerService)
    {
        _ledgerService = ledgerService;
    }

    [HttpGet("trial-balance")]
    public async Task<ActionResult<IReadOnlyList<TrialBalanceRowDto>>> GetTrialBalance([FromQuery] Guid periodId, CancellationToken ct)
    {
        return Ok(await _ledgerService.GetTrialBalanceAsync(periodId, ct));
    }

    [HttpGet("trial-balance-post")]
    public async Task<ActionResult<IReadOnlyList<TrialBalanceRowDto>>> GetTrialBalancePost([FromQuery] Guid periodId, CancellationToken ct)
    {
        return Ok(await _ledgerService.GetTrialBalancePostAsync(periodId, ct));
    }

    [HttpGet("income-statement")]
    public async Task<ActionResult<IReadOnlyList<IncomeStatementRowDto>>> GetIncomeStatement([FromQuery] Guid periodId, CancellationToken ct)
    {
        return Ok(await _ledgerService.GetIncomeStatementAsync(periodId, ct));
    }
}
