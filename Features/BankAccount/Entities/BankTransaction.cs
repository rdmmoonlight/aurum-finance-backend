namespace Aurum.Api.Features.BankAccount.Entities;

/// <summary>Maps onto the existing "bank_transactions" table — no schema change.</summary>
public sealed class BankTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid BankAccountId { get; set; }

    public DateTime TransactionDateUtc { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public BankTransactionType Type { get; set; }

    /// <summary>User-set reconciliation tag.</summary>
    public string? Category { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
