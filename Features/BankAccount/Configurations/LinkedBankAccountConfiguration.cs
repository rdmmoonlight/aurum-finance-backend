using Aurum.Api.Features.BankAccount.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Api.Features.BankAccount.Configurations;

/// <summary>
/// Maps onto the pre-existing "bank_accounts" table exactly as created by
/// the original Drizzle schema — no CreateTable/AlterTable migration
/// should be generated for this entity.
/// </summary>
public sealed class LinkedBankAccountConfiguration : IEntityTypeConfiguration<LinkedBankAccount>
{
    public void Configure(EntityTypeBuilder<LinkedBankAccount> builder)
    {
        builder.ToTable("bank_accounts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(a => a.Provider).HasColumnName("provider").HasColumnType("bank_provider").HasDefaultValue(BankProvider.Bri);
        builder.Property(a => a.AccountHolderName).HasColumnName("account_holder_name").IsRequired();
        builder.Property(a => a.AccountNumber).HasColumnName("account_number").HasMaxLength(34).IsRequired();
        builder.Property(a => a.Balance).HasColumnName("balance").HasColumnType("numeric(18,2)").HasDefaultValue(0m);
        builder.Property(a => a.LastSyncedAtUtc).HasColumnName("last_synced_at");
        builder.Property(a => a.CreatedAtUtc).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAtUtc).HasColumnName("updated_at");

        builder.HasIndex(a => a.UserId).IsUnique().HasDatabaseName("bank_accounts_user_id_uq");
    }
}
