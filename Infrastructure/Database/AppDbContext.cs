using Aurum.Api.Features.Accounting.Accounts.Entities;
using Aurum.Api.Features.Accounting.Periods.Entities;
using Aurum.Api.Features.Authentication.Entities;
using Aurum.Api.Features.BankAccount.Entities;
using Aurum.Api.Features.Journals.Entities;
using Aurum.Api.Features.Ledger.Configurations;
using Aurum.Api.Features.Ledger.Views;
using Aurum.Api.Features.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Infrastructure.Database;

/// <summary>
/// Primary EF Core database context for the application, backed by PostgreSQL.
/// Entity sets are added here as each feature module introduces its own
/// persistence model (e.g. Users, Journals, Ledger entries).
///
/// IMPORTANT: Accounts and Periods map onto tables that already existed
/// before this API — see each entity's Configuration class. Migrations
/// generated from this context must not include CreateTable/AlterTable for
/// "accounts" or "periods" — strip those operations out before applying if
/// EF's migration scaffolding adds them (this can happen if a migration is
/// generated before the model "knows" the tables already exist in a target
/// database).
///
/// "users" is a genuinely new table; the auth-hardening pass that added
/// refresh tokens, password reset, and email verification introduced three
/// more genuinely new tables (refresh_tokens, email_verification_tokens,
/// password_reset_tokens) plus new nullable/defaulted columns on "users"
/// (full_name replaces the old display_name, role, is_active,
/// email_confirmed_at_utc, last_login_at_utc, failed_login_attempts,
/// locked_until_utc). Run `dotnet ef migrations add AddAuthHardening` and
/// review the generated migration before applying it against a database
/// that already has an old "users" table with a "display_name" column.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Authentication / Users feature — genuinely new tables.
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    // Accounting feature — map onto pre-existing tables, no schema change.
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Period> Periods => Set<Period>();

    // Journals feature — maps onto pre-existing journal_entries table, no schema change.
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    // Ledger feature — read-only, maps onto pre-existing views.
    public DbSet<TrialBalanceRow> TrialBalanceRows => Set<TrialBalanceRow>();

    public DbSet<TrialBalancePostRow> TrialBalancePostRows => Set<TrialBalancePostRow>();

    public DbSet<IncomeStatementRow> IncomeStatementRows => Set<IncomeStatementRow>();

    // BankAccount feature — maps onto pre-existing bank_accounts/bank_transactions tables, no schema change.
    public DbSet<LinkedBankAccount> LinkedBankAccounts => Set<LinkedBankAccount>();

    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Declare native Postgres enum types so EF Core's migration
        // scaffolding recognizes them (they already exist in the database —
        // created by the original Drizzle schema, not by this context).
        modelBuilder.HasPostgresEnum<AccountRole>("account_role");
        modelBuilder.HasPostgresEnum<PeriodStatus>("period_status");
        modelBuilder.HasPostgresEnum<JournalKind>("journal_kind");
        modelBuilder.HasPostgresEnum<BankProvider>("bank_provider");
        modelBuilder.HasPostgresEnum<BankTransactionType>("bank_transaction_type");

        // Read-only keyless entities mapped onto pre-existing views —
        // configured explicitly (not auto-discovered like
        // IEntityTypeConfiguration<T> below) since one static class configures
        // all three related view entities together.
        LedgerViewConfigurations.Configure(modelBuilder);

        // Apply entity configurations from this assembly as feature modules
        // add IEntityTypeConfiguration<T> classes alongside their entities.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
