using Aurum.Api.Core.Extensions;
using Aurum.Api.Features.BankAccount.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aurum.Api.Features.BankAccount;

[ApiController]
[Authorize]
[Route("api/bank-account")]
public sealed class BankAccountController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;
    private readonly IValidator<LinkAccountRequest> _linkValidator;
    private readonly IValidator<UpdateTransactionRequest> _updateTransactionValidator;

    public BankAccountController(
        IBankAccountService bankAccountService,
        IValidator<LinkAccountRequest> linkValidator,
        IValidator<UpdateTransactionRequest> updateTransactionValidator)
    {
        _bankAccountService = bankAccountService;
        _linkValidator = linkValidator;
        _updateTransactionValidator = updateTransactionValidator;
    }

    [HttpGet]
    public async Task<ActionResult<BankAccountStatusResponse>> GetStatus(CancellationToken ct)
    {
        return Ok(await _bankAccountService.GetStatusAsync(ct));
    }

    [HttpPost("link")]
    public async Task<ActionResult<BankAccountStatusResponse>> Link([FromBody] LinkAccountRequest request, CancellationToken ct)
    {
        await _linkValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        return Ok(await _bankAccountService.LinkAsync(request, ct));
    }

    [HttpPost("sync")]
    public async Task<ActionResult<BankAccountStatusResponse>> Sync(CancellationToken ct)
    {
        return Ok(await _bankAccountService.SyncAsync(ct));
    }

    [HttpPatch("transactions/{id:guid}")]
    public async Task<ActionResult<BankTransactionDto>> UpdateTransaction(Guid id, [FromBody] UpdateTransactionRequest request, CancellationToken ct)
    {
        await _updateTransactionValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        return Ok(await _bankAccountService.UpdateTransactionAsync(id, request, ct));
    }
}
