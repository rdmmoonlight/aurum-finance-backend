namespace Aurum.Api.Features.Ledger.Views;

/// <summary>
/// Keyless entity mapped onto the pre-existing "vw_trial_balance" Postgres
/// view. UserId/PeriodId exist on the underlying view (used to filter, per
/// packages/db/views.ts::queryView.trialBalance's WHERE clause) but aren't
/// part of the DTO returned to the client — see Ledger/Dtos/TrialBalanceRowDto.
/// </summary>
public sealed class TrialBalanceRow
{
    public Guid UserId { get; set; }

    public Guid PeriodId { get; set; }

    public string AccountRef { get; set; } = string.Empty;

    public string AccountName { get; set; } = string.Empty;

    public decimal TotalDebit { get; set; }

    public decimal TotalCredit { get; set; }

    public decimal NetBalance { get; set; }
}

/// <summary>Same shape as TrialBalanceRow — mapped onto "vw_trial_balance_post" instead.</summary>
public sealed class TrialBalancePostRow
{
    public Guid UserId { get; set; }

    public Guid PeriodId { get; set; }

    public string AccountRef { get; set; } = string.Empty;

    public string AccountName { get; set; } = string.Empty;

    public decimal TotalDebit { get; set; }

    public decimal TotalCredit { get; set; }

    public decimal NetBalance { get; set; }
}

/// <summary>
/// Keyless entity mapped onto the pre-existing "vw_income_statement" view.
/// Column shape matches packages/db/views.ts::queryView.incomeStatement.
/// </summary>
public sealed class IncomeStatementRow
{
    public Guid UserId { get; set; }

    public Guid PeriodId { get; set; }

    public string AccountRef { get; set; } = string.Empty;

    public string AccountName { get; set; } = string.Empty;

    public decimal TotalDebit { get; set; }

    public decimal TotalCredit { get; set; }

    public decimal Net { get; set; }
}
