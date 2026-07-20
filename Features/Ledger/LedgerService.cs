using Aurum.Api.Features.Ledger.Dtos;
using Aurum.Api.Infrastructure.Database;
using Aurum.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Features.Ledger;

public interface ILedgerService
{
    Task<IReadOnlyList<TrialBalanceRowDto>> GetTrialBalanceAsync(Guid periodId, CancellationToken ct = default);

    Task<IReadOnlyList<TrialBalanceRowDto>> GetTrialBalancePostAsync(Guid periodId, CancellationToken ct = default);

    Task<IReadOnlyList<IncomeStatementRowDto>> GetIncomeStatementAsync(Guid periodId, CancellationToken ct = default);
}

/// <summary>
/// New feature — NestJS never exposed an HTTP endpoint for these queries
/// (packages/db/views.ts::queryView existed but had no caller anywhere in
/// the Nest backend or the Next.js frontend). The three underlying
/// Postgres views (vw_trial_balance, vw_trial_balance_post,
/// vw_income_statement) already existed and already did the aggregation;
/// this service just exposes them over HTTP for the first time, using the
/// exact same filter (user_id + period_id) and column selection as
/// queryView did, so it's ready to plug into a Next.js ledger screen.
/// </summary>
public sealed class LedgerService : ILedgerService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public LedgerService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TrialBalanceRowDto>> GetTrialBalanceAsync(Guid periodId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        return await _db.TrialBalanceRows
            .Where(r => r.UserId == userId && r.PeriodId == periodId)
            .OrderBy(r => r.AccountRef)
            .Select(r => new TrialBalanceRowDto
            {
                AccountRef = r.AccountRef,
                AccountName = r.AccountName,
                TotalDebit = r.TotalDebit,
                TotalCredit = r.TotalCredit,
                NetBalance = r.NetBalance,
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TrialBalanceRowDto>> GetTrialBalancePostAsync(Guid periodId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        return await _db.TrialBalancePostRows
            .Where(r => r.UserId == userId && r.PeriodId == periodId)
            .OrderBy(r => r.AccountRef)
            .Select(r => new TrialBalanceRowDto
            {
                AccountRef = r.AccountRef,
                AccountName = r.AccountName,
                TotalDebit = r.TotalDebit,
                TotalCredit = r.TotalCredit,
                NetBalance = r.NetBalance,
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<IncomeStatementRowDto>> GetIncomeStatementAsync(Guid periodId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        return await _db.IncomeStatementRows
            .Where(r => r.UserId == userId && r.PeriodId == periodId)
            .OrderBy(r => r.AccountRef)
            .Select(r => new IncomeStatementRowDto
            {
                AccountRef = r.AccountRef,
                AccountName = r.AccountName,
                TotalDebit = r.TotalDebit,
                TotalCredit = r.TotalCredit,
                Net = r.Net,
            })
            .ToListAsync(ct);
    }
}
