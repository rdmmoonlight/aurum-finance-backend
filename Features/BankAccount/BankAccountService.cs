using Aurum.Api.Core.Exceptions;
using Aurum.Api.Features.BankAccount.Dtos;
using Aurum.Api.Features.BankAccount.Entities;
using Aurum.Api.Infrastructure.Database;
using Aurum.Api.Infrastructure.External.Bri;
using Aurum.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Features.BankAccount;

public interface IBankAccountService
{
    Task<BankAccountStatusResponse> GetStatusAsync(CancellationToken ct = default);

    Task<BankAccountStatusResponse> LinkAsync(LinkAccountRequest request, CancellationToken ct = default);

    Task<BankAccountStatusResponse> SyncAsync(CancellationToken ct = default);

    Task<BankTransactionDto> UpdateTransactionAsync(Guid id, UpdateTransactionRequest request, CancellationToken ct = default);
}

/// <summary>
/// Ported 1:1 from NestJS's BankAccountService
/// (backend/src/bank-account/bank-account.service.ts).
/// </summary>
public sealed class BankAccountService : IBankAccountService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IBriClient _bri;

    public BankAccountService(AppDbContext db, ICurrentUserService currentUser, IBriClient bri)
    {
        _db = db;
        _currentUser = currentUser;
        _bri = bri;
    }

    public async Task<BankAccountStatusResponse> GetStatusAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var account = await _db.LinkedBankAccounts.FirstOrDefaultAsync(a => a.UserId == userId, ct);
        if (account is null)
        {
            return new BankAccountStatusResponse { IsLinked = false, Account = null };
        }

        var transactions = await _db.BankTransactions
            .Where(t => t.BankAccountId == account.Id)
            .OrderByDescending(t => t.TransactionDateUtc)
            .Take(50)
            .ToListAsync(ct);

        return new BankAccountStatusResponse
        {
            IsLinked = true,
            Account = ToDto(account),
            Transactions = transactions.Select(ToDto).ToList(),
        };
    }

    public async Task<BankAccountStatusResponse> LinkAsync(LinkAccountRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var result = await _bri.InquireAccountAsync(request.AccountHolderName, request.AccountNumber, ct);
        if (!result.Verified)
        {
            throw new BadRequestException("BRI could not verify this account. Check the details and try again.");
        }

        var existing = await _db.LinkedBankAccounts.FirstOrDefaultAsync(a => a.UserId == userId, ct);

        LinkedBankAccount account;
        if (existing is not null)
        {
            existing.AccountHolderName = request.AccountHolderName;
            existing.AccountNumber = request.AccountNumber;
            existing.Balance = result.Balance;
            existing.LastSyncedAtUtc = DateTime.UtcNow;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            account = existing;
        }
        else
        {
            account = new LinkedBankAccount
            {
                UserId = userId,
                Provider = Entities.BankProvider.Bri,
                AccountHolderName = request.AccountHolderName,
                AccountNumber = request.AccountNumber,
                Balance = result.Balance,
                LastSyncedAtUtc = DateTime.UtcNow,
            };
            _db.LinkedBankAccounts.Add(account);
        }

        await _db.SaveChangesAsync(ct);

        await SyncTransactionsAsync(account.Id, request.AccountNumber, userId, ct);
        return await GetStatusAsync(ct);
    }

    public async Task<BankAccountStatusResponse> SyncAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var account = await _db.LinkedBankAccounts.FirstOrDefaultAsync(a => a.UserId == userId, ct)
            ?? throw new NotFoundException("No bank account is linked yet.");

        var result = await _bri.InquireAccountAsync(account.AccountHolderName, account.AccountNumber, ct);
        account.Balance = result.Balance;
        account.LastSyncedAtUtc = DateTime.UtcNow;
        account.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await SyncTransactionsAsync(account.Id, account.AccountNumber, userId, ct);
        return await GetStatusAsync(ct);
    }

    public async Task<BankTransactionDto> UpdateTransactionAsync(Guid id, UpdateTransactionRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;

        var transaction = await _db.BankTransactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct)
            ?? throw new NotFoundException("Transaction not found.");

        if (request.Category.IsSet)
        {
            transaction.Category = request.Category.Value;
        }

        if (request.Notes.IsSet)
        {
            transaction.Notes = request.Notes.Value;
        }

        await _db.SaveChangesAsync(ct);
        return ToDto(transaction);
    }

    private async Task SyncTransactionsAsync(Guid bankAccountId, string accountNumber, Guid userId, CancellationToken ct)
    {
        var mutations = await _bri.FetchMutationsAsync(accountNumber, ct);
        if (mutations.Count == 0)
        {
            return;
        }

        var transactions = mutations.Select(m => new Entities.BankTransaction
        {
            UserId = userId,
            BankAccountId = bankAccountId,
            TransactionDateUtc = m.TransactionDateUtc,
            Description = m.Description,
            Amount = m.Amount,
            Type = m.Type == "credit" ? Entities.BankTransactionType.Credit : Entities.BankTransactionType.Debit,
        }).ToList();

        _db.BankTransactions.AddRange(transactions);
        await _db.SaveChangesAsync(ct);
    }

    private static BankAccountDto ToDto(LinkedBankAccount account) => new()
    {
        Id = account.Id,
        UserId = account.UserId,
        Provider = account.Provider == Entities.BankProvider.Bri ? "BRI" : account.Provider.ToString(),
        AccountHolderName = account.AccountHolderName,
        AccountNumber = account.AccountNumber,
        Balance = account.Balance,
        LastSyncedAt = account.LastSyncedAtUtc,
        CreatedAt = account.CreatedAtUtc,
        UpdatedAt = account.UpdatedAtUtc,
    };

    private static BankTransactionDto ToDto(Entities.BankTransaction transaction) => new()
    {
        Id = transaction.Id,
        UserId = transaction.UserId,
        BankAccountId = transaction.BankAccountId,
        TransactionDate = transaction.TransactionDateUtc,
        Description = transaction.Description,
        Amount = transaction.Amount,
        Type = transaction.Type == Entities.BankTransactionType.Credit ? "credit" : "debit",
        Category = transaction.Category,
        Notes = transaction.Notes,
        CreatedAt = transaction.CreatedAtUtc,
    };
}
