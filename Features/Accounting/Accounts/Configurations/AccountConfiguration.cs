using Aurum.Api.Features.Accounting.Accounts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Api.Features.Accounting.Accounts.Configurations;

/// <summary>
/// Maps onto the pre-existing "accounts" table exactly as created by the
/// original Drizzle schema — column names/types/indexes below must match
/// what's already in the database. This configuration does not, and must
/// not, generate a CreateTable/AlterTable migration for this entity; see
/// the root README's "Same database schema" note.
/// </summary>
public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(a => a.Ref).HasColumnName("ref").HasMaxLength(10).IsRequired();
        builder.Property(a => a.Name).HasColumnName("name").IsRequired();
        builder.Property(a => a.Role).HasColumnName("role").HasColumnType("account_role");
        builder.Property(a => a.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(a => a.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(a => a.CreatedAtUtc).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAtUtc).HasColumnName("updated_at");

        builder.HasIndex(a => new { a.UserId, a.Ref })
            .IsUnique()
            .HasDatabaseName("accounts_user_id_ref_uq");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("accounts_user_id_idx");
    }
}
