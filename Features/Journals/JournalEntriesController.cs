using Aurum.Api.Core.Extensions;
using Aurum.Api.Core.Shared;
using Aurum.Api.Features.Journals.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedOkResult = Aurum.Api.Core.Shared.OkResult;

namespace Aurum.Api.Features.Journals;

[ApiController]
[Authorize]
[Route("api/journal-entries")]
public sealed class JournalEntriesController : ControllerBase
{
    private readonly IJournalEntriesService _journalEntriesService;
    private readonly IValidator<CreateJournalEntryRequest> _createValidator;
    private readonly IValidator<UpdateJournalEntryRequest> _updateValidator;

    public JournalEntriesController(
        IJournalEntriesService journalEntriesService,
        IValidator<CreateJournalEntryRequest> createValidator,
        IValidator<UpdateJournalEntryRequest> updateValidator)
    {
        _journalEntriesService = journalEntriesService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<ActionResult<IReadOnlyList<JournalEntryDto>>> Create([FromBody] CreateJournalEntryRequest request, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        return Ok(await _journalEntriesService.CreateAsync(request, ct));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JournalEntryDto>>> FindAll([FromQuery] Guid? periodId, CancellationToken ct)
    {
        return Ok(await _journalEntriesService.FindAllAsync(periodId, ct));
    }

    [HttpGet("{groupId:guid}")]
    public async Task<ActionResult<IReadOnlyList<JournalEntryDto>>> FindOne(Guid groupId, CancellationToken ct)
    {
        return Ok(await _journalEntriesService.FindOneAsync(groupId, ct));
    }

    [HttpPatch("{groupId:guid}")]
    public async Task<ActionResult<IReadOnlyList<JournalEntryDto>>> Update(Guid groupId, [FromBody] UpdateJournalEntryRequest request, CancellationToken ct)
    {
        await _updateValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        return Ok(await _journalEntriesService.UpdateAsync(groupId, request, ct));
    }

    [HttpDelete("{groupId:guid}")]
    public async Task<ActionResult<Aurum.Api.Core.Shared.OkResult>> Remove(Guid groupId, CancellationToken ct)
    {
        await _journalEntriesService.RemoveAsync(groupId, ct);
        return Ok(new Aurum.Api.Core.Shared.OkResult());
    }
}
