using Aurum.Api.Core.Extensions;
using Aurum.Api.Core.Shared;
using Aurum.Api.Features.Accounting.Periods.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedOkResult = Aurum.Api.Core.Shared.OkResult;

namespace Aurum.Api.Features.Accounting.Periods;

[ApiController]
[Authorize]
[Route("api/periods")]
public sealed class PeriodsController : ControllerBase
{
    private readonly IPeriodsService _periodsService;
    private readonly IValidator<CreatePeriodRequest> _createValidator;
    private readonly IValidator<UpdatePeriodRequest> _updateValidator;

    public PeriodsController(
        IPeriodsService periodsService,
        IValidator<CreatePeriodRequest> createValidator,
        IValidator<UpdatePeriodRequest> updateValidator)
    {
        _periodsService = periodsService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<ActionResult<PeriodDto>> Create([FromBody] CreatePeriodRequest request, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        return Ok(await _periodsService.CreateAsync(request, ct));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PeriodDto>>> FindAll(CancellationToken ct)
    {
        return Ok(await _periodsService.FindAllAsync(ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PeriodDto>> FindOne(Guid id, CancellationToken ct)
    {
        return Ok(await _periodsService.FindOneAsync(id, ct));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<PeriodDto>> Update(Guid id, [FromBody] UpdatePeriodRequest request, CancellationToken ct)
    {
        await _updateValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        return Ok(await _periodsService.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Aurum.Api.Core.Shared.OkResult>> Remove(Guid id, CancellationToken ct)
    {
        await _periodsService.RemoveAsync(id, ct);
        return Ok(new Aurum.Api.Core.Shared.OkResult());
    }
}
