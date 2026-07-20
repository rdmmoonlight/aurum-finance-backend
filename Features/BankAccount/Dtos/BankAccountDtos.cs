namespace Aurum.Api.Features.BankAccount.Dtos;

public sealed class BankAccountDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string Provider { get; init; } = string.Empty;

    public string AccountHolderName { get; init; } = string.Empty;

    public string AccountNumber { get; init; } = string.Empty;

    public decimal Balance { get; init; }

    public DateTime? LastSyncedAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}

public sealed class BankTransactionDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public Guid BankAccountId { get; init; }

    public DateTime TransactionDate { get; init; }

    public string Description { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Type { get; init; } = string.Empty;

    public string? Category { get; init; }

    public string? Notes { get; init; }

    public DateTime CreatedAt { get; init; }
}

/// <summary>Response for GET /api/bank-account.</summary>
public sealed class BankAccountStatusResponse
{
    public bool IsLinked { get; init; }

    public BankAccountDto? Account { get; init; }

    /// <summary>Only present when IsLinked is true — up to the 50 most recent transactions.</summary>
    public List<BankTransactionDto>? Transactions { get; init; }
}

public sealed class LinkAccountRequest
{
    public string AccountHolderName { get; init; } = string.Empty;

    /// <summary>BRI account numbers are numeric, 10-15 digits in sandbox.</summary>
    public string AccountNumber { get; init; } = string.Empty;
}

public sealed class UpdateTransactionRequest
{
    public Aurum.Api.Core.Shared.Optional<string> Category { get; init; }

    public Aurum.Api.Core.Shared.Optional<string> Notes { get; init; }
}
