namespace Aurum.Api.Features.Accounting.Accounts.Dtos;

public sealed class ReorderAccountsRequest
{
    public List<Guid> OrderedIds { get; init; } = new();
}
