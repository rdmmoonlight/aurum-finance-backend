using NpgsqlTypes;

namespace Aurum.Api.Features.BankAccount.Entities;

/// <summary>Mirrors the existing native Postgres enum "bank_provider" — only "BRI" exists today.</summary>
public enum BankProvider
{
    [PgName("BRI")]
    Bri,
}

/// <summary>Mirrors the existing native Postgres enum "bank_transaction_type".</summary>
public enum BankTransactionType
{
    [PgName("credit")]
    Credit,

    [PgName("debit")]
    Debit,
}
