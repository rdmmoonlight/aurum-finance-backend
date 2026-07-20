using Aurum.Api.Core.Extensions;
using Aurum.Api.Core.Shared;
using Aurum.Api.Features.Accounting.Accounts.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OkResult = Aurum.Api.Core.Shared.OkResult;

namespace Aurum.Api.Features.Accounting.Accounts;

/// <summary>
/// Ported 1:1 from NestJS's AccountsController. Route ordering note carried
/// over verbatim: "reorder" and "reset" must stay registered before
/// "{id}" — otherwise routing would match PATCH /accounts/reorder as
/// PATCH /accounts/{id} with id="reorder", exactly the same trap Nest's
/// own comment warns about.
/// </summary>
[ApiController]
[Authorize]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly IAccountsService _accountsService;
    private readonly IValidator<CreateAccountRequest> _createValidator;
    private readonly IValidator<UpdateAccountRequest> _updateValidator;
    private readonly IValidator<ReorderAccountsRequest> _reorderValidator;

    public AccountsController(
        IAccountsService accountsService,
        IValidator<CreateAccountRequest> createValidator,
        IValidator<UpdateAccountRequest> updateValidator,
        IValidator<ReorderAccountsRequest> reorderValidator)
    {
        _accountsService = accountsService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _reorderValidator = reorderValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> FindAll(CancellationToken ct)
    {
        return Ok(await _accountsService.FindAllAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        return Ok(await _accountsService.CreateAsync(request, ct));
    }

    [HttpPatch("reorder")]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> Reorder([FromBody] ReorderAccountsRequest request, CancellationToken ct)
    {
        await _reorderValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        return Ok(await _accountsService.ReorderAsync(request, ct));
    }

    [HttpPost("reset")]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> Reset([FromBody] ResetAccountsRequest request, CancellationToken ct)
    {
        return Ok(await _accountsService.ResetAsync(request, ct));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<AccountDto>> Update(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken ct)
    {
        await _updateValidator.ValidateAndThrowAppExceptionAsync(request, ct);
        return Ok(await _accountsService.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<OkResult>> Remove(Guid id, CancellationToken ct)
    {
        await _accountsService.RemoveAsync(id, ct);
        return Ok(new OkResult());
    }
}
