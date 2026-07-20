namespace Aurum.Api.Features.Reports.Dtos;

public sealed class MonthSummaryDto
{
    public string My { get; init; } = string.Empty;

    public Guid PeriodId { get; init; }

    public string Status { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;

    public decimal SeedCash { get; init; }

    public decimal SeedBank { get; init; }

    public decimal Revenue { get; init; }

    public decimal OperatingRevenue { get; init; }

    public decimal OtherRevenue { get; init; }

    public decimal Expense { get; init; }

    public decimal OperatingExpense { get; init; }

    public decimal OtherExpense { get; init; }

    public decimal Profit { get; init; }

    public decimal Loss { get; init; }

    public bool IsLoss { get; init; }

    public decimal NetIncome { get; init; }
}

public sealed class AnnualTotalsDto
{
    public decimal TotalRevenue { get; init; }

    public decimal TotalExpense { get; init; }

    public decimal TotalProfit { get; init; }

    public decimal TotalLoss { get; init; }

    public decimal NetAnnual { get; init; }

    public int ProfitableMonths { get; init; }

    public int LossMonths { get; init; }

    public int PeriodCount { get; init; }
}

public sealed class PeriodSummaryDto
{
    public Guid Id { get; init; }

    public string My { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}

public sealed class AnnualSummaryResponse
{
    public string Year { get; init; } = string.Empty;

    public List<MonthSummaryDto> Months { get; init; } = new();

    public AnnualTotalsDto Totals { get; init; } = new();

    public List<PeriodSummaryDto> Periods { get; init; } = new();
}
