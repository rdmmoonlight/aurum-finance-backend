namespace Aurum.Api.Features.Accounting.Accounts.Dtos;

public sealed class ResetAccountsRequest
{
    public Guid? ActivePeriodId { get; init; }
}
