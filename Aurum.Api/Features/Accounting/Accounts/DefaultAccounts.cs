using Aurum.Api.Features.Accounting.Accounts.Entities;

namespace Aurum.Api.Features.Accounting.Accounts;

public sealed record DefaultAccount(string Ref, string Name, AccountRole? Role = null);

/// <summary>
/// Ported 1:1 from the NestJS backend's accounts/default-accounts.ts, which
/// itself was kept in manual sync with apps/web/data/default-accounts.ts.
/// Used only by AccountsService.ResetAsync — keep this list identical to
/// the Nest source; don't add/reorder/rename entries without updating both.
/// </summary>
public static class DefaultAccounts
{
    public static readonly IReadOnlyList<DefaultAccount> All = new List<DefaultAccount>
    {
        // Asset (101-199)
        new("101", "Cash"),
        new("102", "Bank"),

        // Equity (300-399)
        new("302", "Income Summary", AccountRole.Clearing),
        new("303", "Retained Earnings"),

        // Revenue (400-499)
        new("401", "Salary Income"),
        new("402", "Family Income"),
        new("409", "Other Income"),

        // Expense (500-799)
        new("501", "Daily Living Expenses"),
        new("502", "Transportation Expenses"),
        new("503", "Household Expenses"),
        new("504", "Social Expenses"),
        new("509", "Other Expenses"),
    };
}
