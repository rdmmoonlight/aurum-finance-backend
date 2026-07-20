using Aurum.Api.Features.Journals.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Api.Features.Journals.Configurations;

/// <summary>
/// Maps onto the pre-existing "journal_entries" table exactly as created by
/// the original Drizzle schema. Does not generate a CreateTable/AlterTable
/// migration for this entity — see the root README's "Same database
/// schema" note. No FK navigation to Period is declared here deliberately —
/// period_id is treated as a plain column so EF never tries to
/// validate/manage that relationship's constraint.
/// </summary>
public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries");

        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).HasColumnName("id");
        builder.Property(j => j.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(j => j.PeriodId).HasColumnName("period_id").IsRequired();
        builder.Property(j => j.Kind).HasColumnName("kind").HasColumnType("journal_kind").IsRequired();
        builder.Property(j => j.GroupId).HasColumnName("group_id").IsRequired();
        builder.Property(j => j.Date).HasColumnName("date").IsRequired();
        builder.Property(j => j.AccountRef).HasColumnName("account_ref").HasMaxLength(10).IsRequired();
        builder.Property(j => j.AccountName).HasColumnName("account_name").IsRequired();
        builder.Property(j => j.Description).HasColumnName("description").HasDefaultValue(string.Empty);
        builder.Property(j => j.Debit).HasColumnName("debit").HasColumnType("numeric(18,2)");
        builder.Property(j => j.Credit).HasColumnName("credit").HasColumnType("numeric(18,2)");
        builder.Property(j => j.IsClosingTemp).HasColumnName("is_closing_temp").HasDefaultValue(false);
        builder.Property(j => j.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(j => j.CreatedAtUtc).HasColumnName("created_at");
        builder.Property(j => j.UpdatedAtUtc).HasColumnName("updated_at");

        builder.HasIndex(j => new { j.UserId, j.PeriodId }).HasDatabaseName("journal_entries_user_id_period_id_idx");
        builder.HasIndex(j => new { j.UserId, j.PeriodId, j.Kind }).HasDatabaseName("journal_entries_user_id_period_id_kind_idx");
        builder.HasIndex(j => new { j.UserId, j.GroupId }).HasDatabaseName("journal_entries_user_id_group_id_idx");
        builder.HasIndex(j => new { j.PeriodId, j.Date }).HasDatabaseName("journal_entries_period_id_date_idx");
        builder.HasIndex(j => j.PeriodId).HasDatabaseName("journal_entries_period_id_idx");
    }
}
