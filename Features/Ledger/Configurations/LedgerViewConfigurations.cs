using Aurum.Api.Features.Ledger.Views;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Features.Ledger.Configurations;

/// <summary>
/// Maps three keyless entities onto three pre-existing Postgres views
/// (vw_trial_balance, vw_trial_balance_post, vw_income_statement) — created
/// by the original schema, not by this API. .ToView() + .HasNoKey() means
/// EF Core never generates any migration for these; they're read-only.
/// </summary>
public static class LedgerViewConfigurations
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrialBalanceRow>(builder =>
        {
            builder.HasNoKey();
            builder.ToView("vw_trial_balance");
            builder.Property(r => r.UserId).HasColumnName("user_id");
            builder.Property(r => r.PeriodId).HasColumnName("period_id");
            builder.Property(r => r.AccountRef).HasColumnName("account_ref");
            builder.Property(r => r.AccountName).HasColumnName("account_name");
            builder.Property(r => r.TotalDebit).HasColumnName("total_debit");
            builder.Property(r => r.TotalCredit).HasColumnName("total_credit");
            builder.Property(r => r.NetBalance).HasColumnName("net_balance");
        });

        modelBuilder.Entity<TrialBalancePostRow>(builder =>
        {
            builder.HasNoKey();
            builder.ToView("vw_trial_balance_post");
            builder.Property(r => r.UserId).HasColumnName("user_id");
            builder.Property(r => r.PeriodId).HasColumnName("period_id");
            builder.Property(r => r.AccountRef).HasColumnName("account_ref");
            builder.Property(r => r.AccountName).HasColumnName("account_name");
            builder.Property(r => r.TotalDebit).HasColumnName("total_debit");
            builder.Property(r => r.TotalCredit).HasColumnName("total_credit");
            builder.Property(r => r.NetBalance).HasColumnName("net_balance");
        });

        modelBuilder.Entity<IncomeStatementRow>(builder =>
        {
            builder.HasNoKey();
            builder.ToView("vw_income_statement");
            builder.Property(r => r.UserId).HasColumnName("user_id");
            builder.Property(r => r.PeriodId).HasColumnName("period_id");
            builder.Property(r => r.AccountRef).HasColumnName("account_ref");
            builder.Property(r => r.AccountName).HasColumnName("account_name");
            builder.Property(r => r.TotalDebit).HasColumnName("total_debit");
            builder.Property(r => r.TotalCredit).HasColumnName("total_credit");
            builder.Property(r => r.Net).HasColumnName("net");
        });
    }
}
