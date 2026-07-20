using Aurum.Api.Core.Exceptions;
using Aurum.Api.Features.Accounting.Accounts.Dtos;
using Aurum.Api.Features.Accounting.Accounts.Entities;
using Aurum.Api.Infrastructure.Database;
using Aurum.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Features.Accounting.Accounts;

public interface IAccountsService
{
    Task<IReadOnlyList<AccountDto>> FindAllAsync(CancellationToken ct = default);

    Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken ct = default);

    Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct = default);

    Task RemoveAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<AccountDto>> ReorderAsync(ReorderAccountsRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<AccountDto>> ResetAsync(ResetAccountsRequest request, CancellationToken ct = default);
}

/// <summary>
/// Ported 1:1 from NestJS's AccountsService (backend/src/accounts/accounts.service.ts).
/// Business rules kept identical: unique (userId, ref), block delete when
/// referenced by journal entries, block reset when the active period
/// already has transactions.
/// </summary>
public sealed class AccountsService : IAccountsService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AccountsService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AccountDto>> FindAllAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var accounts = await _db.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.SortOrder)
            .ToListAsync(ct);

        return accounts.Select(ToDto).ToList();
    }

    public async Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        await AssertRefIsAvailableAsync(userId, request.Ref, excludeId: null, ct);

        var account = new Account
        {
            UserId = userId,
            Ref = request.Ref,
            Name = request.Name,
            Role = ParseRole(request.Role),
            SortOrder = request.SortOrder ?? 0,
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync(ct);

        return ToDto(account);
    }

    public async Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        var account = await FindOwnedOrThrowAsync(id, userId, ct);

        if (request.Ref.IsSet && request.Ref.Value != account.Ref)
        {
            await AssertRefIsAvailableAsync(userId, request.Ref.Value!, excludeId: id, ct);
            account.Ref = request.Ref.Value!;
        }

        if (request.Name.IsSet)
        {
            account.Name = request.Name.Value!;
        }

        // Role: null explicitly clears the role; to set it, always send the
        // field (there is no "leave unchanged" default once you include it).
        if (request.Role.IsSet)
        {
            account.Role = ParseRole(request.Role.Value);
        }

        if (request.SortOrder.IsSet)
        {
            account.SortOrder = request.SortOrder.Value;
        }

        account.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return ToDto(account);
    }

    public async Task RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        var account = await FindOwnedOrThrowAsync(id, userId, ct);

        var hasJournalEntries = await _db.JournalEntries
            .AnyAsync(j => j.UserId == userId && j.AccountRef == account.Ref, ct);

        if (hasJournalEntries)
        {
            throw new BadRequestException("Account already has journal entries and cannot be deleted.");
        }

        _db.Accounts.Remove(account);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AccountDto>> ReorderAsync(ReorderAccountsRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var accounts = await _db.Accounts
            .Where(a => a.UserId == userId && request.OrderedIds.Contains(a.Id))
            .ToListAsync(ct);

        var accountsById = accounts.ToDictionary(a => a.Id);

        for (var index = 0; index < request.OrderedIds.Count; index++)
        {
            if (accountsById.TryGetValue(request.OrderedIds[index], out var account))
            {
                account.SortOrder = index;
                account.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync(ct);
        return await FindAllAsync(ct);
    }

    public async Task<IReadOnlyList<AccountDto>> ResetAsync(ResetAccountsRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        if (request.ActivePeriodId is Guid activePeriodId)
        {
            var hasEntries = await _db.JournalEntries
                .AnyAsync(j => j.PeriodId == activePeriodId && j.UserId == userId, ct);

            if (hasEntries)
            {
                throw new BadRequestException("Reset unavailable: the active period still has transactions.");
            }
        }

        var existing = await _db.Accounts.Where(a => a.UserId == userId).ToListAsync(ct);
        _db.Accounts.RemoveRange(existing);

        var defaults = DefaultAccounts.All
            .Select((defaultAccount, index) => new Account
            {
                UserId = userId,
                Ref = defaultAccount.Ref,
                Name = defaultAccount.Name,
                Role = defaultAccount.Role,
                SortOrder = index,
            })
            .ToList();

        _db.Accounts.AddRange(defaults);
        await _db.SaveChangesAsync(ct);

        return await FindAllAsync(ct);
    }

    private async Task AssertRefIsAvailableAsync(Guid userId, string ref_, Guid? excludeId, CancellationToken ct)
    {
        var query = _db.Accounts.Where(a => a.UserId == userId && a.Ref == ref_);
        if (excludeId is Guid id)
        {
            query = query.Where(a => a.Id != id);
        }

        if (await query.AnyAsync(ct))
        {
            throw new ConflictException($"Account ref \"{ref_}\" is already in use.");
        }
    }

    private async Task<Account> FindOwnedOrThrowAsync(Guid id, Guid userId, CancellationToken ct)
    {
        return await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct)
            ?? throw new NotFoundException("Account not found.");
    }

    private static AccountRole? ParseRole(string? role) => role switch
    {
        null => null,
        "clearing" => AccountRole.Clearing,
        _ => throw new BadRequestException($"Unknown account role \"{role}\"."),
    };

    private static AccountDto ToDto(Account account) => new()
    {
        Id = account.Id,
        UserId = account.UserId,
        Ref = account.Ref,
        Name = account.Name,
        Role = account.Role switch { AccountRole.Clearing => "clearing", _ => null },
        IsActive = account.IsActive,
        SortOrder = account.SortOrder,
        CreatedAt = account.CreatedAtUtc,
        UpdatedAt = account.UpdatedAtUtc,
    };
}
