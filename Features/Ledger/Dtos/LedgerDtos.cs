namespace Aurum.Api.Features.Ledger.Dtos;

/// <summary>Shared shape for trial balance / trial balance (post-close) rows.</summary>
public sealed class TrialBalanceRowDto
{
    public string AccountRef { get; init; } = string.Empty;

    public string AccountName { get; init; } = string.Empty;

    public decimal TotalDebit { get; init; }

    public decimal TotalCredit { get; init; }

    public decimal NetBalance { get; init; }
}

public sealed class IncomeStatementRowDto
{
    public string AccountRef { get; init; } = string.Empty;

    public string AccountName { get; init; } = string.Empty;

    public decimal TotalDebit { get; init; }

    public decimal TotalCredit { get; init; }

    public decimal Net { get; init; }
}
