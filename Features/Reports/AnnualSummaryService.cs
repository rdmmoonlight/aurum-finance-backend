using System.Text.RegularExpressions;
using Aurum.Api.Core.Exceptions;
using Aurum.Api.Features.Journals.Entities;
using Aurum.Api.Features.Reports.Dtos;
using Aurum.Api.Infrastructure.Database;
using Aurum.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Features.Reports;

public interface IAnnualSummaryService
{
    Task<AnnualSummaryResponse> FindByYearAsync(string? year, CancellationToken ct = default);
}

/// <summary>
/// Ported 1:1 from NestJS's AnnualSummaryService
/// (backend/src/annual-summary/annual-summary.service.ts). Recomputes the
/// summary in application code rather than querying the pre-existing
/// vw_annual_summary Postgres view — same as the original, which also
/// aggregated in JS rather than using that view. See this feature's README
/// for the tradeoff.
/// </summary>
public sealed partial class AnnualSummaryService : IAnnualSummaryService
{
    private static readonly string[] MonthShort =
    {
        "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
    };

    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex YearPattern();

    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AnnualSummaryService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AnnualSummaryResponse> FindByYearAsync(string? year, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(year) || !YearPattern().IsMatch(year))
        {
            throw new BadRequestException("A valid 4-digit \"year\" query parameter is required.");
        }

        var userId = _currentUser.UserId;

        var allPeriods = await _db.Periods
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);

        // periods.my is stored as "MM-YYYY"
        var yearPeriods = allPeriods
            .Where(p => p.My.Split('-').ElementAtOrDefault(1) == year)
            .OrderBy(p => int.Parse(p.My.Split('-')[0]))
            .ToList();

        var periodIds = yearPeriods.Select(p => p.Id).ToList();

        var entries = periodIds.Count > 0
            ? await _db.JournalEntries
                .Where(e => e.UserId == userId && periodIds.Contains(e.PeriodId))
                .ToListAsync(ct)
            : new List<JournalEntry>();

        var entriesByPeriod = entries.GroupBy(e => e.PeriodId).ToDictionary(g => g.Key, g => g.ToList());

        var months = yearPeriods.Select(p =>
        {
            var mm = p.My.Split('-')[0];
            var periodEntries = entriesByPeriod.GetValueOrDefault(p.Id, new List<JournalEntry>());

            decimal operatingRevenue = 0;
            decimal otherRevenue = 0;
            decimal operatingExpense = 0;
            decimal otherExpense = 0;

            foreach (var e in periodEntries)
            {
                // Skip closing entries — they would double-count.
                if (e.Kind == JournalKind.Ce)
                {
                    continue;
                }

                var cls = AccountClassifier.ClassFromRef(e.AccountRef);
                if (cls is null)
                {
                    continue;
                }

                var debit = e.Debit ?? 0;
                var credit = e.Credit ?? 0;

                switch (cls)
                {
                    case "R":
                        operatingRevenue += credit - debit;
                        break;
                    case "OR":
                        otherRevenue += credit - debit;
                        break;
                    case "EXP":
                        operatingExpense += debit - credit;
                        break;
                    case "OE":
                        otherExpense += debit - credit;
                        break;
                }
            }

            var totalRevenue = Math.Max(0, operatingRevenue + otherRevenue);
            var totalExpense = Math.Max(0, operatingExpense + otherExpense);
            var difference = totalRevenue - totalExpense;

            var monthIndex = int.Parse(mm);

            return new MonthSummaryDto
            {
                My = p.My,
                PeriodId = p.Id,
                Status = StatusToString(p.Status),
                Label = monthIndex >= 0 && monthIndex < MonthShort.Length ? MonthShort[monthIndex] : mm,
                SeedCash = p.SeedCash,
                SeedBank = p.SeedBank,
                Revenue = totalRevenue,
                OperatingRevenue = Math.Max(0, operatingRevenue),
                OtherRevenue = Math.Max(0, otherRevenue),
                Expense = totalExpense,
                OperatingExpense = Math.Max(0, operatingExpense),
                OtherExpense = Math.Max(0, otherExpense),
                Profit = difference > 0 ? difference : 0,
                Loss = difference < 0 ? Math.Abs(difference) : 0,
                IsLoss = difference < 0,
                NetIncome = Math.Abs(difference),
            };
        }).ToList();

        var totals = new AnnualTotalsDto
        {
            TotalRevenue = months.Sum(m => m.Revenue),
            TotalExpense = months.Sum(m => m.Expense),
            TotalProfit = months.Where(m => !m.IsLoss).Sum(m => m.NetIncome),
            TotalLoss = months.Where(m => m.IsLoss).Sum(m => m.NetIncome),
            NetAnnual = months.Sum(m => m.IsLoss ? -m.NetIncome : m.NetIncome),
            ProfitableMonths = months.Count(m => !m.IsLoss && m.Revenue > 0),
            LossMonths = months.Count(m => m.IsLoss),
            PeriodCount = months.Count,
        };

        return new AnnualSummaryResponse
        {
            Year = year,
            Months = months,
            Totals = totals,
            Periods = yearPeriods.Select(p => new PeriodSummaryDto
            {
                Id = p.Id,
                My = p.My,
                Status = StatusToString(p.Status),
            }).ToList(),
        };
    }

    private static string StatusToString(Accounting.Periods.Entities.PeriodStatus status) => status switch
    {
        Accounting.Periods.Entities.PeriodStatus.Open => "open",
        Accounting.Periods.Entities.PeriodStatus.Closing => "closing",
        Accounting.Periods.Entities.PeriodStatus.Closed => "closed",
        _ => throw new InvalidOperationException($"Unhandled period status {status}."),
    };
}
