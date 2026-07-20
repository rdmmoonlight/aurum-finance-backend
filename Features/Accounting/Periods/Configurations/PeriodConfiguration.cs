using Aurum.Api.Features.Accounting.Periods.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Api.Features.Accounting.Periods.Configurations;

/// <summary>
/// Maps onto the pre-existing "periods" table exactly as created by the
/// original Drizzle schema. Does not generate a CreateTable/AlterTable
/// migration for this entity — see the root README's "Same database
/// schema" note.
/// </summary>
public sealed class PeriodConfiguration : IEntityTypeConfiguration<Period>
{
    public void Configure(EntityTypeBuilder<Period> builder)
    {
        builder.ToTable("periods");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(p => p.My).HasColumnName("my").HasMaxLength(7).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasColumnType("period_status").HasDefaultValue(PeriodStatus.Open);
        builder.Property(p => p.SeedCash).HasColumnName("seed_cash").HasColumnType("numeric(18,2)").HasDefaultValue(0m);
        builder.Property(p => p.SeedBank).HasColumnName("seed_bank").HasColumnType("numeric(18,2)").HasDefaultValue(0m);
        builder.Property(p => p.ClosedAtUtc).HasColumnName("closed_at");
        builder.Property(p => p.CreatedAtUtc).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAtUtc).HasColumnName("updated_at");

        builder.HasIndex(p => new { p.UserId, p.My })
            .IsUnique()
            .HasDatabaseName("periods_user_id_my_uq");

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("periods_user_id_idx");
    }
}
