using Aurum.Api.Features.Reports.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Features.Reports;

[ApiController]
[Authorize]
[Route("api/annual-summary")]
public sealed class AnnualSummaryController : ControllerBase
{
    private readonly IAnnualSummaryService _annualSummaryService;

    public AnnualSummaryController(IAnnualSummaryService annualSummaryService)
    {
        _annualSummaryService = annualSummaryService;
    }

    [HttpGet]
    public async Task<ActionResult<AnnualSummaryResponse>> FindByYear([FromQuery] string? year, CancellationToken ct)
    {
        return Ok(await _annualSummaryService.FindByYearAsync(year, ct));
    }
}
