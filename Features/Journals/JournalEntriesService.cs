using Aurum.Api.Common.PeriodLock;
using Aurum.Api.Core.Exceptions;
using Aurum.Api.Features.Accounting.Periods.Entities;
using Aurum.Api.Features.Journals.Dtos;
using Aurum.Api.Features.Journals.Entities;
using Aurum.Api.Infrastructure.Database;
using Aurum.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Features.Journals;

public interface IJournalEntriesService
{
    Task<IReadOnlyList<JournalEntryDto>> CreateAsync(CreateJournalEntryRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<JournalEntryDto>> FindAllAsync(Guid? periodId, CancellationToken ct = default);

    Task<IReadOnlyList<JournalEntryDto>> FindOneAsync(Guid groupId, CancellationToken ct = default);

    Task<IReadOnlyList<JournalEntryDto>> UpdateAsync(Guid groupId, UpdateJournalEntryRequest request, CancellationToken ct = default);

    Task RemoveAsync(Guid groupId, CancellationToken ct = default);
}

/// <summary>
/// Ported 1:1 from NestJS's JournalEntriesService
/// (backend/src/journal-entries/journal-entries.service.ts). This is the
/// core bookkeeping engine — every write goes through PeriodLockPolicy.AssertMutable
/// exactly where the Nest service called assertPeriodMutable.
/// </summary>
public sealed class JournalEntriesService : IJournalEntriesService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public JournalEntriesService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<JournalEntryDto>> CreateAsync(CreateJournalEntryRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        var period = await GetPeriodOrThrowAsync(request.PeriodId, userId, ct);
        var kind = ParseKind(request.Kind);

        PeriodLockPolicy.AssertMutable(period, kind);

        var entries = request.Rows.Select((row, index) => new JournalEntry
        {
            UserId = userId,
            PeriodId = request.PeriodId,
            Kind = kind,
            GroupId = request.GroupId,
            Date = request.Date,
            AccountRef = row.AccountRef,
            AccountName = row.AccountName,
            Description = row.Description ?? string.Empty,
            Debit = row.Debit.HasValue ? decimal.Round(row.Debit.Value, 2) : null,
            Credit = row.Credit.HasValue ? decimal.Round(row.Credit.Value, 2) : null,
            SortOrder = index,
        }).ToList();

        _db.JournalEntries.AddRange(entries);
        await _db.SaveChangesAsync(ct);

        return entries.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<JournalEntryDto>> FindAllAsync(Guid? periodId, CancellationToken ct = default)
    {
        if (periodId is null)
        {
            return Array.Empty<JournalEntryDto>();
        }

        var userId = _currentUser.UserId;

        var entries = await _db.JournalEntries
            .Where(j => j.PeriodId == periodId && j.UserId == userId)
            .OrderBy(j => j.Date).ThenBy(j => j.SortOrder)
            .ToListAsync(ct);

        return entries.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<JournalEntryDto>> FindOneAsync(Guid groupId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var entries = await _db.JournalEntries
            .Where(j => j.GroupId == groupId && j.UserId == userId)
            .OrderBy(j => j.SortOrder)
            .ToListAsync(ct);

        if (entries.Count == 0)
        {
            throw new NotFoundException("Journal entry group not found.");
        }

        return entries.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<JournalEntryDto>> UpdateAsync(Guid groupId, UpdateJournalEntryRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var existing = await _db.JournalEntries
            .Where(j => j.GroupId == groupId && j.UserId == userId)
            .ToListAsync(ct);

        if (existing.Count == 0)
        {
            throw new NotFoundException("Journal entry group not found.");
        }

        var period = await GetPeriodOrThrowAsync(existing[0].PeriodId, userId, ct);
        PeriodLockPolicy.AssertMutable(period, existing[0].Kind);

        var existingIds = existing.Select(e => e.Id).ToHashSet();
        foreach (var row in request.Rows)
        {
            if (!existingIds.Contains(row.Id))
            {
                throw new BadRequestException($"Row {row.Id} does not belong to this journal entry.");
            }
        }

        var byId = existing.ToDictionary(e => e.Id);
        var updated = new List<JournalEntry>();

        foreach (var row in request.Rows)
        {
            var entry = byId[row.Id];
            entry.Date = request.Date;
            entry.AccountRef = row.AccountRef;
            entry.AccountName = row.AccountName;
            entry.Description = row.Description ?? string.Empty;
            entry.Debit = row.Debit.HasValue ? decimal.Round(row.Debit.Value, 2) : null;
            entry.Credit = row.Credit.HasValue ? decimal.Round(row.Credit.Value, 2) : null;
            entry.UpdatedAtUtc = DateTime.UtcNow;
            updated.Add(entry);
        }

        await _db.SaveChangesAsync(ct);

        return updated.Select(ToDto).ToList();
    }

    public async Task RemoveAsync(Guid groupId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var existing = await _db.JournalEntries
            .Where(j => j.GroupId == groupId && j.UserId == userId)
            .ToListAsync(ct);

        if (existing.Count == 0)
        {
            throw new NotFoundException("Journal entry group not found.");
        }

        var period = await GetPeriodOrThrowAsync(existing[0].PeriodId, userId, ct);
        PeriodLockPolicy.AssertMutable(period, existing[0].Kind);

        _db.JournalEntries.RemoveRange(existing);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Period> GetPeriodOrThrowAsync(Guid periodId, Guid userId, CancellationToken ct)
    {
        return await _db.Periods.FirstOrDefaultAsync(p => p.Id == periodId && p.UserId == userId, ct)
            ?? throw new NotFoundException("Period not found.");
    }

    private static JournalKind ParseKind(string kind) => kind switch
    {
        "GJ" => JournalKind.Gj,
        "AE" => JournalKind.Ae,
        "CE" => JournalKind.Ce,
        _ => throw new BadRequestException($"Unknown journal kind \"{kind}\"."),
    };

    private static string KindToString(JournalKind kind) => kind switch
    {
        JournalKind.Gj => "GJ",
        JournalKind.Ae => "AE",
        JournalKind.Ce => "CE",
        _ => throw new InvalidOperationException($"Unhandled journal kind {kind}."),
    };

    private static JournalEntryDto ToDto(JournalEntry entry) => new()
    {
        Id = entry.Id,
        UserId = entry.UserId,
        PeriodId = entry.PeriodId,
        Kind = KindToString(entry.Kind),
        GroupId = entry.GroupId,
        Date = entry.Date,
        AccountRef = entry.AccountRef,
        AccountName = entry.AccountName,
        Description = entry.Description,
        Debit = entry.Debit,
        Credit = entry.Credit,
        IsClosingTemp = entry.IsClosingTemp,
        SortOrder = entry.SortOrder,
        CreatedAt = entry.CreatedAtUtc,
        UpdatedAt = entry.UpdatedAtUtc,
    };
}
