namespace Aurum.Api.Features.BankAccount.Entities;

/// <summary>
/// Maps onto the existing "bank_accounts" table — no schema change.
/// Named LinkedBankAccount (not BankAccount) to avoid colliding with this
/// feature's own folder/namespace name (Features.BankAccount).
/// </summary>
public sealed class LinkedBankAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>One linked account per user — unique on UserId.</summary>
    public Guid UserId { get; set; }

    public BankProvider Provider { get; set; } = BankProvider.Bri;

    public string AccountHolderName { get; set; } = string.Empty;

    public string AccountNumber { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public DateTime? LastSyncedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
