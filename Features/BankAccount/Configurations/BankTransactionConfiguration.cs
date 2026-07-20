using Aurum.Api.Features.BankAccount.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Api.Features.BankAccount.Configurations;

/// <summary>
/// Maps onto the pre-existing "bank_transactions" table exactly as created
/// by the original Drizzle schema. No FK navigation to LinkedBankAccount is
/// declared — bank_account_id is a plain column, same reasoning as
/// JournalEntry's period_id (see that entity's configuration comment).
/// </summary>
public sealed class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.ToTable("bank_transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(t => t.BankAccountId).HasColumnName("bank_account_id").IsRequired();
        builder.Property(t => t.TransactionDateUtc).HasColumnName("transaction_date").IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").IsRequired();
        builder.Property(t => t.Amount).HasColumnName("amount").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(t => t.Type).HasColumnName("type").HasColumnType("bank_transaction_type").IsRequired();
        builder.Property(t => t.Category).HasColumnName("category");
        builder.Property(t => t.Notes).HasColumnName("notes");
        builder.Property(t => t.CreatedAtUtc).HasColumnName("created_at");

        builder.HasIndex(t => t.UserId).HasDatabaseName("bank_transactions_user_id_idx");
        builder.HasIndex(t => t.BankAccountId).HasDatabaseName("bank_transactions_bank_account_id_idx");
    }
}
